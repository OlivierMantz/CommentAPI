# CommentAPI
## API Quality state
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=OlivierMantz_CommentAPI&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=OlivierMantz_CommentAPI)

[![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=OlivierMantz_CommentAPI)](https://sonarcloud.io/summary/new_code?id=OlivierMantz_CommentAPI)

[![Lint Code, Build, Test and Publish](https://github.com/OlivierMantz/CommentAPI/actions/workflows/pipeline.yaml/badge.svg?branch=dev&event=push)](https://github.com/OlivierMantz/CommentAPI/actions/workflows/pipeline.yaml)

## Creating SQLite Database
```
sqlite3 Comment.db
```
## Creating table
```
CREATE TABLE Comments (
    Id INTEGER PRIMARY KEY,
    Content TEXT NOT NULL,
    UserId TEXT NOT NULL,
    PostId INTEGER NOT NULL
);

```
## Checking tables 
```
.tables
```
