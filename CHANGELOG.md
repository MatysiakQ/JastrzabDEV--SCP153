# Changelog

## 2.2.0 — 2026-10-30

- HS: 400 base, +50 maximum/current per confirmed bite, 2000 cap; regeneration restores the earned maximum.
- Fresh per-assignment state, immediate role initialization, cancellable round spawn, isolated role registration/unregistration.
- Confirm damage in Hurt using the original handler and victim life; no rewards or teleports from cancelled attacks.
- Scope bite effects to native Zombie attack execution; one reward per swing. Patch server cooldown rather than adding another Hurting timer.
- Suppress only SCP-153's native Zombie shield regeneration; respect native blocking and actual damage timestamps.
- Replace delayed spectator callbacks with observer reconciliation; visible Zombie fallback if the schematic fails or disappears.
- Check walkable ground and capsule clearance; remove unvalidated teleport fallback, inaccessible room candidates, and post-warhead displacement.
- Validate configuration and role-ID ownership; monotonic configurable evolution.
- Preserve shared fields/effects where still owned; reject duplicate assignment and overriding another CustomRole.
- Use version 2.2.0 consistently; add SDK build, regression tests, CI and runtime verification checklist.

Client animation prediction, map geometry, model hitboxes and integration with the exact deployed server remain subject to in-game validation.
