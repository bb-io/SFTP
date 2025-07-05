# Blackbird.io SFTP

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

SFTP, or Secure File Transfer Protocol, is a secure file transfer protocol that uses secure shell encryption to provide a high level of security for sending and receiving file transfers.
To use SFTP, you need to have an SFTP server, where files can be uploaded, stored, and retrieved in a downloadable format. An SFTP server is the type of storage location where files are stored and retrieved.

## Before setting up

Before you can connect you need to make sure that:

- You have a SFTP server and you have the credentials to access it.

## Connecting

1. Navigate to Apps, and identify the **SFTP** app. You can use search to find it.
2. Click _Add Connection_.
3. Name your connection for future reference e.g. 'SFTP connection'.
4. Fill in the **Host** of your SFTP server.
5. Fill in the **Port** of your SFTP server (usually it's 22).
6. Fill in the **Username** of user who has access to SFTP server.
7. Fill in the **Password** of user who has access to SFTP server.
8. Click _Connect_.

![connection](image/README/connection.png)

## Actions

### Files

- **Upload file** Upload files to server by specified path.
- **Download file** Download file from server by path.
- **Delete file** Delete a file from server by specified path.
- **Search files** List files (name and full path) by specified path
- **Rename file** Rename a file by specified path from old to new

### Folders

- **Create folder** Create new folder by specified path.
- **Delete folder** Delete folder from server by specified path.

## Events

### Files

- **On files updated** This polling event triggers when a file is created or updated on server.
- **On files deleted** This polling event triggers when a file is deleted from server.

### Folders

- **On folders created** This polling event triggers when folders are created within specified time interval.

## Example 

Here is an example of how you can use the SFTP app in a workflow:

![example](image/README/example.png)

In this example, the workflow starts with the **On files created or updated** event, which triggers when any file is added or updated on SFTP server. Then, the workflow uses the **Download file** action to download the file that was added/updated. In the next step we translate the file via `DeepL` and then upload the translated file to Slack channel.

## Eggs

Check downloadable workflow prototypes featuring this app that you can import to your Nests [here](https://docs.blackbird.io/eggs/storage-to-mt/). 

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
