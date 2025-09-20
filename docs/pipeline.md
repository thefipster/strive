# Pipeline

## Stages

### Scanner

* Input: string -> Directory paths to scan for files
* Output: ImportIndex -> Version, Filepath, Timestamp, Hash
    * Only output files that are new or have changed
    * Override to output all files

### Classifier

* Input: ImportIndex
* Output: ClassificationIndex -> Version, Filepath, Timestamp, Source, Date, Range



### Extractor

* Input: ClassificationIndex
* Output ExtractionIndex -> Version, Filepath, Timestamp, SourceFile, Source, Date, Range, Hash, Metric Parameters, Series Parameters
  * Only outputs files that are new or have changed
  * Override to output all files

