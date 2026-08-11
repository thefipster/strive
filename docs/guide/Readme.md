# Export guides

How to get a full data export out of every provider Strive ingests. One guide per provider, each
written as a checklist you can follow end to end, ending with what to hand to Strive's upload page.

| Provider | Guide | Route | Typical wait | Package |
|---|---|---|---|---|
| Polar | [polar.md](polar.md) | `account.polar.com` → *Download your data* | hours to days | ZIP of JSON |
| Garmin | [garmin.md](garmin.md) | Garmin Account → *Export Your Data* | 24–48 h, up to 30 days | ZIP of FIT + JSON |
| Withings | [withings.md](withings.md) | Health dashboard → *Download my data* | minutes to hours | ZIP of CSV |
| Google Maps Timeline | [google-maps-timeline.md](google-maps-timeline.md) | on-device export in Maps/Android settings | immediate | single JSON |
| Oral-B | [oral-b.md](oral-b.md) | in-app export under *EU Data Act* | immediate | single JSON |

## Ground rules

- **Never rename or repack an export.** Strive deduplicates by SHA-256 of the file bytes and records
  every path an archive contained, so the original ZIP is the most useful thing you can upload — it
  carries the vendor's own folder structure, which the classifiers use as a hint.
- **Re-exporting is cheap and safe.** Overlapping packages collapse to one catalog entry per unique
  file; re-uploading the identical archive is detected and does no work
  ([step 1](../roadmap/step-1-upload-dedup.md)). Requesting a fresh full export every few months is
  the intended backup rhythm.
- **Download links expire.** Garmin's is the tightest (3 days), Polar's is two weeks. Grab the file
  the day the mail arrives.
- **Keep the mail.** The export mail is the only record of when the snapshot was taken; the archive
  itself often has no export timestamp.
- **Unparsed is not lost.** Formats Strive cannot read yet (Garmin FIT, Withings EKG) are still
  hashed, cataloged and stored, and surface in the unknown/deferred queue until a parser exists.
  Export everything anyway.

## A note on freshness

Provider UIs move. Every guide carries a *Verified* line with the date and the sources it was
checked against; if a menu path no longer matches, follow the linked support article and please fix
the guide in the same pass.
