// app.js (Final Version with All Features)
document.addEventListener('DOMContentLoaded', () => {
    // --- State Variables ---
    let myPrivateKey = null;
    let myPublicKey = null;
    let partnerPublicKey = null;
    let chatHistory = [];
    let isViewingArchive = false;
    const connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
    const archiveStorageKey = 'quantumGuardArchives';

    // --- UI Elements ---
    const initialSection = document.getElementById('initial-section');
    const chatSection = document.getElementById('chat-section');
    const generateKeysBtn = document.getElementById('generate-keys-btn');
    const myPrivateKeyElem = document.getElementById('my-private-key');
    const myPublicKeyElem = document.getElementById('my-public-key');
    const partnerPublicKeyElem = document.getElementById('partner-public-key');
    const startChatBtn = document.getElementById('start-chat-btn');
    const chatMessages = document.getElementById('chat-messages');
    const messageInputArea = document.getElementById('message-input-area');
    const messageInput = document.getElementById('message-input');
    const sendButton = document.getElementById('send-button');
    const copyPublicKeyBtn = document.getElementById('copy-public-key-btn');
    const copyPrivateKeyBtn = document.getElementById('copy-private-key-btn');
    const endSessionBtn = document.getElementById('end-session-btn');
    const savedSessionsList = document.getElementById('saved-sessions-list');
    const chatSessionTitle = document.getElementById('chat-session-title');

    // --- Archive Management ---
    function getArchives() {
        const archives = localStorage.getItem(archiveStorageKey);
        return archives ? JSON.parse(archives) : {};
    }

    function saveArchive(name, encryptedHistory, partnerKey, ownPublicKey) {
        const archives = getArchives();
        archives[name] = { encryptedHistory, partnerKey, myPublicKey: ownPublicKey };
        localStorage.setItem(archiveStorageKey, JSON.stringify(archives));
    }

    function deleteSession(name) {
        if (confirm(`Are you sure you want to permanently delete the session "${name}"? This cannot be undone.`)) {
            const archives = getArchives();
            delete archives[name];
            localStorage.setItem(archiveStorageKey, JSON.stringify(archives));
            loadArchivesToUI();
        }
    }

    function loadArchivesToUI() {
        const archives = getArchives();
        savedSessionsList.innerHTML = '';
        const names = Object.keys(archives);

        if (names.length === 0) {
            savedSessionsList.innerHTML = '<p class="text-center text-gray-500">No saved sessions found.</p>';
            return;
        }

        names.forEach(name => {
            const div = document.createElement('div');
            div.className = 'flex justify-between items-center bg-gray-700 p-2 rounded-lg';
            
            const nameSpan = document.createElement('span');
            nameSpan.className = 'font-semibold';
            nameSpan.textContent = name;
            
            const buttonGroup = document.createElement('div');

            const loadBtn = document.createElement('button');
            loadBtn.textContent = 'Load';
            loadBtn.className = 'bg-blue-600 hover:bg-blue-500 text-white font-bold py-1 px-3 rounded-lg text-sm';
            loadBtn.onclick = () => loadSession(name);

            const deleteBtn = document.createElement('button');
            deleteBtn.innerHTML = '<i class="fas fa-trash-alt"></i> Delete';
            deleteBtn.className = 'bg-red-700 hover:bg-red-600 text-white font-bold py-1 px-3 rounded-lg text-sm ml-2';
            deleteBtn.onclick = () => deleteSession(name);

            buttonGroup.appendChild(loadBtn);
            buttonGroup.appendChild(deleteBtn);
            div.appendChild(nameSpan);
            div.appendChild(buttonGroup);
            savedSessionsList.appendChild(div);
        });
    }

    async function loadSession(name) {
        const archives = getArchives();
        const session = archives[name];
        if (!session) return alert('Session not found.');

        const providedPrivateKey = prompt(`To unlock and view "${name}", please enter the PRIVATE key for that session:`);
        if (!providedPrivateKey) return;

        try {
            const response = await apiPost('processhistory?action=decrypt', {
                HistoryJson: session.encryptedHistory, MyPrivateKey: providedPrivateKey, OtherPublicKey: session.partnerKey,
            });
            const decryptedHistoryJson = await response.text();
            chatHistory = JSON.parse(decryptedHistoryJson);
            
            isViewingArchive = true;
            initialSection.classList.add('hidden');
            chatSection.classList.remove('hidden');
            
            chatSessionTitle.textContent = `Viewing Archive: ${name}`;
            messageInputArea.style.display = 'none'; // Hide the entire message input area

            chatMessages.innerHTML = '';
            chatHistory.forEach(msg => displayMessage(msg.text, msg.type));
            
        } catch (error) {
            alert('DECRYPTION FAILED. The private key you provided is incorrect for this chat session.');
        }
    }
    
    // --- API & Helper Functions ---
    async function apiPost(endpoint, body) {
        const response = await fetch(`/api/ecc/${endpoint}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body),
        });
        if (!response.ok) {
            console.error(`API Error: ${response.status} ${response.statusText}`);
            throw new Error(`API call to ${endpoint} failed.`);
        }
        return response;
    }

    function displayMessage(message, type) {
        const div = document.createElement('div');
        div.textContent = message;
        div.className = `p-2 my-1 rounded-lg max-w-xs break-words ${type === 'sent' ? 'bg-blue-600 ml-auto' : 'bg-gray-600 mr-auto'}`;
        chatMessages.appendChild(div);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    async function startChatConnection() {
        try {
            isViewingArchive = false;
            if (connection.state !== "Connected") await connection.start();
            await connection.invoke("Register", myPublicKey);
            
            initialSection.classList.add('hidden');
            chatSection.classList.remove('hidden');
            
            chatSessionTitle.textContent = 'Secure Session Active';
            messageInputArea.style.display = 'block'; // Show the message input area
        } catch (err) {
            alert('Could not connect to the chat server.');
        }
    }
    
    // --- SignalR ---
    connection.on("ReceiveMessage", async (senderPublicKey, encryptedMessage) => {
        if (senderPublicKey !== partnerPublicKey) return;
        try {
            const response = await apiPost('decrypt', {
                Message: encryptedMessage, MyPrivateKey: myPrivateKey, OtherPublicKey: senderPublicKey,
            });
            const decryptedMessage = await response.text();
            displayMessage(decryptedMessage, 'received');
            chatHistory.push({ text: decryptedMessage, type: 'received' });
        } catch (error) {
            displayMessage('[DECRYPTION FAILED]', 'received');
        }
    });

    // --- UI Event Listeners ---
    generateKeysBtn.addEventListener('click', async () => {
        try {
            const response = await apiPost('generatekeys', {});
            const keys = await response.json();
            myPrivateKey = keys.privateKey;
            myPublicKey = keys.publicKey;
            myPrivateKeyElem.value = myPrivateKey;
            myPublicKeyElem.value = myPublicKey;
            partnerPublicKey = null;
            partnerPublicKeyElem.value = '';
            chatHistory = [];
            alert('New random keys generated. Share your PUBLIC key.');
        } catch (error) {
            console.error('Failed to generate keys:', error);
            alert('Could not generate keys. Check the developer console (F12) for errors.');
        }
    });

    copyPublicKeyBtn.addEventListener('click', () => {
        myPublicKeyElem.select();
        document.execCommand('copy');
        alert('Public key copied to clipboard!');
    });
    
    copyPrivateKeyBtn.addEventListener('click', () => {
        myPrivateKeyElem.select();
        document.execCommand('copy');
        alert('Private key copied to clipboard! Keep it safe.');
    });

    endSessionBtn.addEventListener('click', async () => {
        if (isViewingArchive) {
            window.location.reload();
            return;
        }

        if (confirm('Do you want to save this chat session permanently?')) {
            const saveName = prompt("Enter a name for this session:");
            if (saveName) {
                try {
                    const historyJson = JSON.stringify(chatHistory);
                    const response = await apiPost('processhistory?action=encrypt', {
                        HistoryJson: historyJson, MyPrivateKey: myPrivateKey, OtherPublicKey: partnerPublicKey,
                    });
                    const encryptedHistory = await response.text();
                    saveArchive(saveName, encryptedHistory, partnerPublicKey, myPublicKey);
                    alert(`Session "${saveName}" saved permanently!`);
                } catch (error) {
                    alert('Could not save the session. It will be discarded.');
                }
            }
        }
        window.location.reload();
    });

    startChatBtn.addEventListener('click', () => {
        partnerPublicKey = partnerPublicKeyElem.value.trim();
        if (!myPublicKey) return alert('Please generate your keys first.');
        if (!partnerPublicKey) return alert("Please paste your partner's key.");
        startChatConnection();
    });
    
    messageInput.addEventListener('keyup', (e) => { if (e.key === 'Enter') sendButton.click(); });

    sendButton.addEventListener('click', async () => {
        const message = messageInput.value.trim();
        if (!message) return;
        try {
            const response = await apiPost('encrypt', {
                Message: message, MyPrivateKey: myPrivateKey, OtherPublicKey: partnerPublicKey
            });
            const encryptedMessage = await response.text();
            await connection.invoke("SendPrivateMessage", partnerPublicKey, encryptedMessage);
            displayMessage(message, 'sent');
            chatHistory.push({ text: message, type: 'sent' });
        } catch (error) {
            alert('Failed to encrypt or send message.');
        }
        messageInput.value = '';
    });

    // --- Initial Load ---
    loadArchivesToUI();
});