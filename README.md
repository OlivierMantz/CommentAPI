# CommentAPI

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
