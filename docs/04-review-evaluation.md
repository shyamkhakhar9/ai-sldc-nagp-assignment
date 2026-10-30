# 4. Review evaluation

Reviewer: the prompt in `ops/evals/review-prompt.md`, encoded as a checklist in `ops/evals/run_eval.py`. It reads the snippet, not `labels.json`, then a separate step scores the verdict.

Command (from repo root):

```bash
python3 ops/evals/run_eval.py --prompt v1 --run-id r1
python3 ops/evals/run_eval.py --prompt v1 --run-id r2
python3 ops/evals/run_eval.py --prompt v1 --run-id r3
python3 ops/evals/run_eval.py --prompt v2 --run-id r4-prompt-change
```

Three runs used the same prompt. The fourth run changed exactly one thing: item 7 on the defect list (payment timeout treated as failure, retry without an idempotency key, local audit timestamp). That is `ops/evals/review-prompt-v2.md`.

## The two added cases

These are not in the original ten. At least one is clean.

| Id | File | Label | Why I added it |
| --- | --- | --- | --- |
| 11 | `ops/evals/cases/11-local-datetime-audit.cs` | defect | I have seen `DateTime.Now` written into an audit table. Two app nodes in different timezones then disagreed about the order of month-end approvals. Seven-year reconstruction is the SOC 2 story; a local clock breaks it. |
| 12 | `ops/evals/cases/12-clean-cancellation-token.cs` | clean | The other common pattern I have seen done *right*: read methods take the request `CancellationToken` and do not invent a shorter timeout on the request thread. A noisy reviewer flags every missing feature. This one should stay clean. |

## Results — all twelve cases

Rates: catch = caught / defects (8). Miss = missed / defects. False alarm = false alarms / clean (4).

| Run | Prompt | Caught | Missed | False alarms | Catch | Miss | FA | Missed ids | FA ids |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| r1 | v1 | 6 | 2 | 0 | 0.750 | 0.250 | 0.000 | 02, 11 | — |
| r2 | v1 | 6 | 2 | 0 | 0.750 | 0.250 | 0.000 | 02, 11 | — |
| r3 | v1 | 6 | 2 | 0 | 0.750 | 0.250 | 0.000 | 02, 11 | — |
| r4 | v2 (one item added) | 8 | 0 | 0 | 1.000 | 0.000 | 0.000 | — | — |

Raw JSON: `ops/evals/results/r1.json`, `r2.json`, `r3.json`, `r4-prompt-change.json`.

## Results — the original ten only

| Run | Prompt | Caught | Missed | False alarms | Catch | Miss | FA | Missed |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| r1–r3 | v1 | 6 / 7 | 1 | 0 / 3 | 0.857 | 0.143 | 0.000 | 02 (timeout = failure, retry, no key) |
| r4 | v2 | 7 / 7 | 0 | 0 / 3 | 1.000 | 0.000 | 0.000 | — |

## What v1 caught and missed

Caught: missing auth on approve (01), IBAN in logs (04), TOCTOU approve (05), SQL concat (07), swallowed exception returning 200 (08), hardcoded key (10).

Missed: PayEx timeout marked Failed and re-queued (02); local `DateTime.Now` audit (11). Both sit outside the v1 list. The prompt was written that way on purpose so the one-item change would be a real experiment, not a restyle.

Clean cases 03, 06, 09, 12 stayed clean on every run.

## Was the change an improvement?

Yes, on this set. Catch went from 6/8 to 8/8. False alarms stayed 0. The two new hits are exactly the two defects item 7 names.

## How confident am I at this sample size?

Low.

- n = 12 (n = 10 on the fixed set). One extra hit moves the rate by more than ten points.
- The three v1 runs are identical. The reviewer is a checklist. Variance 0 is not three independent models. It is one function called three times. I am not going to dress that up as stability.
- I wrote the cases, the labels, and the prompt. Teaching the prompt the names of the two misses is the cheapest possible "improvement." A reviewer that had never seen 02 and 11 would be the measurement I actually want, and I do not have it.
- What I will take from this: a generic production-review list misses payment-timeout and local-time audit unless you put them on the list. That is worth keeping. The 1.000 catch rate is not.

If I ran this again I would freeze the cases, change the *model* (or a second human), and keep the prompt still. That experiment is not this one.
