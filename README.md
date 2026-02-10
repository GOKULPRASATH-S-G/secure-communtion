# QuantumGuard: An End-to-End Encrypted Chat Application


QuantumGuard is a secure, real-time, two-person chat application built on .NET 8 and modern web technologies. This project demonstrates the practical application of **Elliptic Curve Cryptography (ECC)** to establish a secure, end-to-end encrypted communication channel, where the central server acts only as a blind relay for encrypted data and has zero knowledge of the message content.

The application also features an AI-powered sensitivity analysis module that scans messages for potentially sensitive keywords *before* they are encrypted, adding an intelligent layer of user awareness.

## Core Concepts

This project is built on three foundational pillars:

1.  **End-to-End Encryption (E2EE):** The server has zero knowledge of the content of the messages being exchanged.
    *   **Key Generation:** Each user generates a unique Elliptic Curve key pair (a public and private key) directly in their browser.
    *   **Key Exchange:** Users securely derive a shared secret using the **Elliptic Curve Diffie-Hellman (ECDH)** key exchange protocol. This is achieved by exchanging public keys, allowing both users to mathematically compute the same secret without ever sending it over the network.
    *   **Message Encryption:** All messages are encrypted and decrypted client-side using a strong symmetric key (**AES-GCM**) derived from the shared secret.

2.  **Real-time Communication:**
    *   Uses **ASP.NET Core SignalR** to create a persistent, bi-directional connection between the clients and the server.
    *   The server's SignalR Hub maps users by their public keys and instantly relays encrypted messages to the intended recipient without storing or decrypting them.

3.  **AI-Powered Sensitivity Analysis:**
    *   Before a message is sent, it is analyzed by a simulated AI service (`AIService`).
    *   This service uses regular expressions to detect patterns and keywords related to financial, personal (PII), or confidential information.
    *   The result is shown to the sender in real-time, giving them a chance to reconsider sending potentially sensitive data.

## Technology Stack

### Backend
*   **Framework:** ASP.NET Core 8
*   **Language:** C#
*   **Real-time:** SignalR
*   **Cryptography:** `System.Security.Cryptography` for ECDH and AES-GCM

### Frontend
*   **Language:** Vanilla JavaScript (ES6+)
*   **Styling:** Tailwind CSS (via CDN) & Custom CSS for theming
*   **Icons:** Font Awesome

### Deployment
*   **Containerization:** Docker
*   **Hosting Platform:** Render / Railway (or any platform that supports Docker)

## Features

-   **Secure Key Pair Generation:** Create unique and random ECC private/public key pairs on demand.
-   **Real-time Encrypted Messaging:** Instant, secure chat between two users once a session is established.
-   **AI Message Analysis:** Real-time feedback on message sensitivity before encryption.
-   **Persistent & Encrypted Chat Archives:**
    -   Option to save chat history upon ending a session with a custom name.
    -   History is fully encrypted using the session's unique keys and stored securely in the browser's `localStorage`.
-   **Secure Session Management:**
    -   Load saved sessions by providing the correct private key, which is the only key that can decrypt the archive.
    -   Loaded archives are in a secure, **view-only mode** to prevent alteration or sending new messages.
    -   Permanently delete saved sessions from local storage.
-   **Modern User Interface:** A beautiful, responsive UI with a landing page and a clean dashboard layout.

## How to Run Locally

### Prerequisites
-   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Steps

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/GOKULPRASATH-S-G/secure-communtion.git
    ```
2.  **Navigate to the project directory:**
    ```bash
    cd secure-communtion
    ```
3.  **Run the application:**
    ```bash
    dotnet run
    ```
4.  The server will start, typically listening on `http://localhost:5048`. Open this URL in your web browser.

## How to Use the Application

To test the end-to-end encrypted chat, you will need two separate browser windows (e.g., a normal Chrome window and an Incognito window).

1.  **User A (Browser 1):**
    -   Open `http://localhost:5048` and click "Get Started".
    -   On the main app screen, click "Generate New Keys".
    -   Use the "Copy" button to copy the **Public Key**.

2.  **User B (Browser 2):**
    -   Open `http://localhost:5048` and click "Get Started".
    -   Click "Generate New Keys".
    -   Paste User A's public key into the "Enter Your Partner's Public Key" text box.
    -   Use the "Copy" button to copy your (User B's) **Public Key**.

3.  **User A (Browser 1):**
    -   Paste User B's public key into the "Enter Your Partner's Public Key" text box.

4.  **Both Users:**
    -   Click "Start Live Chat".
    -   You can now send and receive end-to-end encrypted messages in real time.

5.  **Saving a Session:**
    -   Click "End Session". You will be prompted to save.
    -   Provide a name for the session to save it permanently and securely in your browser.

## Security Architecture

-   **Stateless Server:** The backend is designed to be stateless. It never stores any private keys, public keys, or message content.
-   **Client-Side Keys:** All private keys are generated and stored exclusively in the client's browser `localStorage` or in memory for the session.
-   **Encrypted Storage:** Saved chat sessions are fully encrypted before being written to the browser's `localStorage`. They can only be decrypted with the original private key used for that session.

## Deployment

This application is configured for deployment using Docker. The `Dockerfile` in the root directory contains a multi-stage build process that:
1.  Builds the .NET application in a .NET SDK container.
2.  Publishes the release artifacts to a final, lightweight ASP.NET runtime container.

This container can be deployed to any modern cloud platform that supports Docker, such as Render, Railway, or Azure App Service.

---
*This project was developed as a demonstration of modern cryptographic principles and secure application design.*
