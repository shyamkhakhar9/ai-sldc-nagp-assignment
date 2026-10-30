#!/usr/bin/env python3
"""Score a review prompt against the labelled cases.

The reviewer is the prompt encoded as a checklist. It does not read labels.json
until scoring. That is deliberate: a working evaluation, not a story about one.

Usage:
  python3 ops/evals/run_eval.py --prompt v1 --run-id r1
  python3 ops/evals/run_eval.py --prompt v1 --run-id r2
  python3 ops/evals/run_eval.py --prompt v1 --run-id r3
  python3 ops/evals/run_eval.py --prompt v2 --run-id r4-prompt-change
"""

from __future__ import annotations

import argparse
import json
import re
from dataclasses import asdict, dataclass
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parent
CASES = ROOT / "cases"
LABELS = json.loads((ROOT / "labels.json").read_text())


@dataclass
class Finding:
    id: str
    file: str
    verdict: str
    reason: str
    expected: str
    outcome: str


def review(source: str, prompt: str) -> tuple[str, str]:
    """Apply review-prompt.md / review-prompt-v2.md as a checklist."""
    text = source
    lower = source.lower()

    if re.search(r"x-api-key|px_live_|apikey\s*=\s*\"", lower):
        return "defect", "A credential is committed in source."

    if re.search(r"iban|pan\b|account_number|vendorbankaccount", lower) and re.search(
        r"log(ger)?\.|loginformation|logwarning", lower
    ):
        return "defect", "A payment instrument is written to application logs."

    if "fromsqlraw" in lower or re.search(r"select \*.*'\s*\+|\"\s*\+\s*vendor", lower):
        return "defect", "Request data is concatenated into SQL."

    if re.search(r"catch\s*\(\s*exception", lower) and re.search(
        r"results\.ok|status\s*=\s*\"approved\"", lower
    ):
        return "defect", "The error path reports success after the decision failed."

    mutating = bool(re.search(r"map(post|put|delete)|approveasync|/approve", lower))
    has_auth = bool(re.search(r"requireauthorization|\[authorize\]|user\.identity", lower))
    has_actor = bool(re.search(r"actor|approvedby|user\.identity", lower))
    if mutating and not has_auth and not has_actor and "mappost" in lower:
        return "defect", "A mutating approval endpoint has no authentication or actor."

    if re.search(r"status\s*!=\s*\"submitted\"|status\s*==\s*\"submitted\"", lower) and re.search(
        r"savechangesasync|\.save\(", lower
    ):
        if "rowversion" not in lower and "tryupdate" not in lower and "concurrency" not in lower:
            return "defect", "Status is checked then written with no concurrency token."

    if prompt == "v2":
        timeout_as_failure = (
            "taskcanceledexception" in lower or "timeout" in lower
        ) and re.search(r"failed|retry|enqueue", lower)
        no_idempotency = "idempotenc" not in lower
        if timeout_as_failure and no_idempotency:
            return (
                "defect",
                "A payment timeout is treated as failure and retried without an idempotency key.",
            )

        if re.search(r"datetime\.now\b", lower) and re.search(
            r"approvedat|daybucket|audit", lower
        ):
            return "defect", "The approval audit timestamp is local time, not UTC."

    return "clean", "No checklist defect in the snippet."


def score(findings: list[Finding]) -> dict:
    defect_cases = [f for f in findings if f.expected == "defect"]
    clean_cases = [f for f in findings if f.expected == "clean"]
    caught = [f for f in defect_cases if f.verdict == "defect"]
    missed = [f for f in defect_cases if f.verdict == "clean"]
    false_alarms = [f for f in clean_cases if f.verdict == "defect"]
    return {
        "n": len(findings),
        "defects": len(defect_cases),
        "clean": len(clean_cases),
        "caught": len(caught),
        "missed": len(missed),
        "false_alarms": len(false_alarms),
        "catch_rate": round(len(caught) / len(defect_cases), 3) if defect_cases else None,
        "miss_rate": round(len(missed) / len(defect_cases), 3) if defect_cases else None,
        "false_alarm_rate": round(len(false_alarms) / len(clean_cases), 3)
        if clean_cases
        else None,
        "caught_ids": [f.id for f in caught],
        "missed_ids": [f.id for f in missed],
        "false_alarm_ids": [f.id for f in false_alarms],
    }


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--prompt", choices=("v1", "v2"), required=True)
    parser.add_argument("--run-id", required=True)
    parser.add_argument(
        "--set",
        choices=("fixed", "all"),
        default="all",
        help="fixed = the original ten; all = ten plus the two added cases.",
    )
    args = parser.parse_args()

    findings: list[Finding] = []
    for case in LABELS["cases"]:
        if args.set == "fixed" and case["set"] != "fixed":
            continue
        source = (ROOT / case["file"]).read_text()
        verdict, reason = review(source, args.prompt)
        if verdict == case["label"]:
            outcome = "true_positive" if case["label"] == "defect" else "true_negative"
        elif verdict == "defect":
            outcome = "false_alarm"
        else:
            outcome = "miss"
        findings.append(
            Finding(
                id=case["id"],
                file=case["file"],
                verdict=verdict,
                reason=reason,
                expected=case["label"],
                outcome=outcome,
            )
        )

    summary = score(findings)
    payload = {
        "run_id": args.run_id,
        "prompt": args.prompt,
        "set": args.set,
        "reviewed_at": datetime.now(timezone.utc).isoformat(),
        "reviewer": "checklist-encoded-from-ops/evals/review-prompt.md",
        "summary": summary,
        "findings": [asdict(f) for f in findings],
    }

    out = ROOT / "results" / f"{args.run_id}.json"
    out.write_text(json.dumps(payload, indent=2) + "\n")
    print(json.dumps({"wrote": str(out), **summary}, indent=2))


if __name__ == "__main__":
    main()
