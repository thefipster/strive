# Export guide — Google Maps Timeline (on-device)

**The path on Android, current as of 2026-08-11:**
**Settings** → **Location** → **Location services** → **Timeline** → **Export Timeline data**

*Verified 2026-08-11 on-device, and against [Manage your Google Maps
Timeline](https://support.google.com/maps/answer/6258979) and the Google Maps community threads on
the on-device Timeline export
([Android](https://support.google.com/maps/thread/311275268), [full JSON
export](https://support.google.com/maps/thread/264641290)).*

**Read this first:** since Google's 2024–2025 Timeline migration, Timeline lives **on your phone**,
not in your Google account. Google Takeout no longer produces a usable Timeline export for migrated
accounts — the *Location History* section is either absent or returns leftovers from before the
migration. The only current route is an export from the device itself.

That has one hard consequence: **this export exists only as long as the phone does.** Lose or wipe
the device without a Timeline backup enabled and the history is gone — Google cannot re-issue it.
Of all the providers here, this is the one worth exporting on a schedule.

## Android

1. Open the phone's **Settings** app.
2. Tap **Location**.
3. Tap **Location services**.
4. Tap **Timeline**.
5. Tap **Export Timeline data**.
6. Tap **Continue**, then **Save**, and choose where to put the file (Downloads, or straight into a
   cloud folder).

On older builds **Timeline** sits directly under **Location**, without the **Location services**
step in between.

The same screen is reachable from Maps: profile picture → **Your Timeline** → **⋯** →
**Location & privacy settings** → **Export Timeline data**.

## iPhone / iPad

1. Open the **Google Maps** app.
2. Tap your **profile picture** in the top-right corner.
3. Tap the **⋯** menu in the top-right corner.
4. Choose **Location and privacy settings**.
5. Tap **Export Timeline data** and save the file (Files app, or share it to yourself).

## What you get

A **single JSON file**, saved locally — no mail, no waiting. The name depends on platform and app
version; both of these are the real thing:

- `Timeline.json` — the usual Android on-device export
- `location-history.json` — seen on iOS and newer Android builds

Some EU accounts get CSV instead of JSON, a data-protection variation on Google's side.

Inside, the substance is the **`semanticSegments`** array: a mix of interpreted visits and
activities (place IDs, activity types, start/end times) and raw GPS point runs. It is *not* the old
`Records.json` / `Semantic Location History` shape from Takeout — if you have Takeout archives from
before the migration, keep and upload those too; they are a different, older format with data the
on-device export does not contain.

Do not confuse `Timeline Edits.json` with `Timeline.json`. The edits file only records manual
corrections and is close to useless on its own.

## Hand it to Strive

This is the one provider that does not hand you an archive, and Strive's upload page accepts
**`.zip` only** — so put the JSON in a ZIP yourself before uploading. Keep the original file name
inside the archive: the Google Timeline classifier is one of the ported legacy detectors
([step 3](../roadmap/step-3-classification.md)), and the name is a useful hint.

```bash
zip timeline-2026-08-10.zip Timeline.json
```

Date the **archive**, not the JSON inside it. Consecutive exports overlap almost entirely and
identical content collapses to one catalog entry, so the archive name is the only thing that tells
you at a glance which snapshot is which. Several exports can go into one ZIP as long as their names
do not collide.

## Rhythm

Monthly, or before any phone change/factory reset. Also turn on Timeline's own encrypted backup in
the same settings screen — that protects against device loss, but it is Google-side and encrypted,
so it is a safety net, not a substitute for these exports.
