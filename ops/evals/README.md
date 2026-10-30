# Review evaluation

Twelve snippets. Ten are the fixed set (`01`–`10`). `11` and `12` were added for this submission: one real defect (local audit timestamps) and one clean (cancellation tokens threaded through reads).

Labels live in `labels.json`. The reviewer never reads them until scoring.

## Command

From the repository root, inside a venv:

```bash
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
python ops/evals/run_eval.py --prompt v1 --run-id r1
python3 ops/evals/run_eval.py --prompt v1 --run-id r2
python3 ops/evals/run_eval.py --prompt v1 --run-id r3
python3 ops/evals/run_eval.py --prompt v2 --run-id r4-prompt-change
```

The reviewer is the prompt in `review-prompt.md` (or `review-prompt-v2.md`) encoded as a checklist. The same command was executed three times on v1, then once on v2 after changing exactly one thing: item 7 on the defect list (timeout / missing idempotency key / local audit timestamp).

Results are JSON in `results/`. The table is in `docs/04-review-evaluation.md`.
