# Export guide — Garmin

*Verified 2026-08-10 against [How Do I Export Data Out of Garmin
Connect?](https://support.garmin.com/en-US/?faq=W1TvTPW8JZ6LfJSfK512Q8), Garmin's data-management
page at <https://www.garmin.com/en-US/account/datamanagement/exportdata/>, [Exporting Garmin Explore
Account Data](https://support.garmin.com/en-US/?faq=C9IsxpGgFU0BGRl5quZJg6), and the Gadgetbridge
[Garmin Connect import notes](https://gadgetbridge.org/basics/topics/garmin/import-garmin-connect/).*

Garmin's bulk export is the closest thing in this list to a Google Takeout: it hands you everything
the account holds, including the original FIT files from every device you ever paired.

## 1. Request the export

1. Sign in at <https://connect.garmin.com>.
2. Click your name or profile picture in the top-right corner → **Account Settings**.
3. In the **Account Management** section, open **Export Your Data**. Depending on your region this
   may be labelled under *Privacy Settings* or *Data Management*.
   Direct link: <https://www.garmin.com/en-US/account/datamanagement/exportdata/>
4. Click **Request Data Export**.

Garmin confirms the request on screen and starts assembling the archive.

## 2. Download it — quickly

- You get a mail with the subject **"Action Required: Download Your Data"** containing the download
  link.
- Most exports arrive **within 24–48 hours**. Garmin's stated GDPR ceiling is **30 days**, and long
  account histories do occasionally take that long.
- **The prepared file is deleted after 3 days.** This is the tightest window of any provider here —
  if the mail lands while you are away, expect to request again.

## 3. What is in the ZIP

One large ZIP, with the interesting parts under `DI_CONNECT/`:

| Folder | Contents |
|---|---|
| `DI_CONNECT/DI-Connect-Uploaded-Files/` | **Nested ZIPs** holding the original activity FIT files |
| `DI_CONNECT/DI-Connect-Fitness/` | activity summaries, JSON |
| `DI_CONNECT/DI-Connect-Wellness/` | daily wellness — steps, heart rate, stress, JSON |
| `DI_CONNECT/DI-Connect-Aggregator/` | aggregated daily/weekly rollups, JSON |
| `DI_CONNECT/DI-Connect-User/` | profile, settings, devices |

The rule of thumb: **activities and heart-rate detail are binary FIT, everything else is JSON**. The
export spans all devices ever connected to the account, not just the current watch.

Note the nested archives — the FIT files sit in ZIPs *inside* the ZIP. You do not need to unpack
them by hand for Strive; hand over the outer archive and let the importer walk it.

### What is not in it

- **Garmin Explore** (inReach, Explore-app tracks and waypoints) is a separate account surface with
  its own export — see the Explore support article linked above if you use it.
- Third-party data that Garmin Connect only *displays* via a partner connection is not Garmin's to
  export.

## 4. Hand it to Strive

Upload the ZIP as-is. Strive currently classifies and catalogs the Garmin files, and **FIT parsing
is a deferred format** — the FIT blobs land in L0 and show up in the unknown/deferred queue until
the reader exists ([step 4](../roadmap/step-4-extraction.md)). Export them now regardless; once a
FIT reader ships, the already-stored blobs are processed without a re-download.

## Per-activity export (no waiting)

For a single activity, skip the bulk request:

1. Open the activity in Garmin Connect on the web.
2. Use the gear / **⋯** menu at the top right.
3. Choose **Export Original** (the FIT file as recorded), or **Export to TCX / GPX / CSV**.

Handy for a spot check, but it is not a substitute for the bulk export — only the bulk archive
carries the wellness and daily-summary JSON.
