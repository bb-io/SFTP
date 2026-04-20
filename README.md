# Blackbird.io File Transfer (SFTP/FTP)

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

This app supports file transfer over both SFTP and FTP.

SFTP, or Secure File Transfer Protocol, uses SSH to provide encrypted file transfers. FTP is a standard file transfer protocol that can be used with servers that expose FTP access.

To use this app, you need access to either an SFTP server or an FTP server where files can be uploaded, stored, and retrieved.

## Before setting up

Before you can connect, make sure that:

- You have either an SFTP server or an FTP server.
- You have the credentials required to access that server.

## Connecting

1. Navigate to Apps, and identify the **File Transfer (SFTP/FTP)** app. You can use search to find it.
2. Click _Add Connection_.
3. Name your connection for future reference, for example `File transfer connection`.
4. Select the connection type:
   - `SFTP`
   - `FTP`
5. Fill in the **Host** of your server.
6. Fill in the **Port** of your server:
   - SFTP usually uses `22`
   - FTP depends on your server configuration
7. Fill in the **Username** of the user who has access to the server.
8. Fill in the authentication value:
   - For `SFTP`, enter either a password or a private key in the **Password or Private Key** field
   - For `FTP`, enter the password in the **Password** field
9. Click _Connect_.

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

Here is an example of how you can use the File Transfer app in a workflow:

![example](image/README/example.png)

In this example, the workflow starts with the **On files created or updated** event, which triggers when any file is added or updated on the connected SFTP or FTP server. Then, the workflow uses the **Download file** action to download the file that was added or updated. In the next step, we translate the file via `DeepL` and then upload the translated file to a Slack channel.

## Eggs

Check downloadable workflow prototypes featuring this app that you can import to your Nests [here](https://docs.blackbird.io/eggs/storage-to-mt/). 

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
