# Ingestion

## Import Files

### Zips

Vendor takeout files uploaded by the user.

### Files

Unzipped files from the uploaded zip archives.

### Extracts

Extracted data from the files.

## Indexes

* ZipIndex
  * Hash based for unique files
* ZipDuplicates
  * Filepath based
  * Hash reference to ZipIndex
* FileIndex
  * Hash based for unique files
* FileDuplicates
  * Filepath based
* SourceIndex
  * Hash based
* ExtractIndex
* DataIndex