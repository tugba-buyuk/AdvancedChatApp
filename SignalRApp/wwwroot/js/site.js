$(function () {

    document.getElementById('uploadButton').addEventListener('click', function () {
        document.getElementById('fileInput').click();
    });

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5269/chathub")
        .withAutomaticReconnect([1000, 2000, 4000]) // parantez içinde ms cinsinden periyotlar belirlenmezse default olarak 0 2 10 30 s aralıklarla istek atar
        .build();

    let activeUser = null;

    connection.start().then(() => {
        console.log("Connected to the SignalR hub.");
    }).catch(err => console.log(`Error while starting connection: ${err}`));

    $("#btnLogin").on("click", () => {
        const accepting = $("#accepting").is(":checked");
        if (accepting) {
            connection.invoke("GetNickName").catch(err => console.log(`Error while sending nickname: ${err}`));
        }
    });

    function formatLastSeen(dateString) {
        const now = new Date();
        const lastSeenDate = new Date(dateString);

        const isToday = now.toDateString() === lastSeenDate.toDateString();
        const options = { hour: '2-digit', minute: '2-digit' };

        if (isToday) {
            // Bugün
            return `Last seen: Today at ${lastSeenDate.toLocaleTimeString([], options)}`;
        } else {
            // Bugün değilse tam tarih ve saat
            return `Last seen: ${lastSeenDate.toLocaleDateString()} at ${lastSeenDate.toLocaleTimeString([], options)}`;
        }
    }

    connection.on("clientPage", otherUsers => {
        $(".accepting-area").addClass("d-none");
        $("#exampleModal").addClass("d-none");
        $(".modal-backdrop").addClass("d-none");
        $(".chat-container").removeClass("d-none");
        $("#_clients").html("");
        $.each(otherUsers, (index, item) => {
            const user = $(".users").first().clone();
            user.removeClass("active");
            user.find("span.fw-bold").text(item.userName);
            user.find("img").attr("src", item.profileImage);
            let lastSeenText = formatLastSeen(item.lastLogin);
            user.find("small.text-muted").text(lastSeenText);
            $("#_clients").append(user);
        });
    });

    $(document).on("click", ".users", function () {
        $(".users").removeClass("active");
        $(this).addClass("active");
        activeUser = $(this);

        // Sağ paneldeki kullanıcı bilgilerini güncelle
        const userName = activeUser.find("span.fw-bold").text();
        const profileImage = activeUser.find("img").attr("src");
        const lastSeen = activeUser.find("small.text-muted").text();

        $(".chat-window h5").text(userName);
        $(".chat-window img").attr("src", profileImage);
        $(".chat-window small").text(lastSeen);

        // Eski mesajları temizle
        $("#chat-messages").empty();

        // Aktif kullanıcı için geçmiş mesajları yükleyin
        connection.invoke("LoadChatHistory", userName).catch(err => console.log(`Error while loading messages: ${err}`));
    });

    $("#messageArea").on("focus", function () {
        activeUser = $(".users.active");
    });
    // Mesaj gönderme
    $("#sendBtn").on("click", async () => {
        const fileInput = document.getElementById("fileInput");
        const file = fileInput.files[0];
        const receiverName = activeUser.find('.info-username').text();

        if (file) {
            const reader = new FileReader();
            reader.onload = async function (e) {
                const base64String = btoa(
                    new Uint8Array(e.target.result)
                        .reduce((data, byte) => data + String.fromCharCode(byte), '')
                );
                try {
                    await connection.invoke("SendFileBase64", file.name, base64String, receiverName);
                } catch (err) {
                    console.error(`Error while sending file: ${err}`);
                }
            };
            reader.readAsArrayBuffer(file);
        } else {
            if (activeUser.length > 0) {
                // TinyMCE'den mesajı almak için tinymce.get() kullanılır
                const messageContent = tinymce.get("messageArea").getContent();

                if (messageContent.trim() !== "") {  // Boş mesaj kontrolü
                    try {
                        await connection.invoke("SendMessage", messageContent, receiverName);
                        tinymce.get("messageArea").setContent(""); // Mesaj alanını temizle
                    } catch (err) {
                        console.error(`Error while sending message: ${err}`);
                    }
                } else {
                    alert("Lütfen bir mesaj yazın.");
                }
            } else {
                alert("Lütfen bir kullanıcı seçin.");
            }
        }
    });


    connection.on("ReceiveFile", (fileName, fileUrl, senderName) => {
        console.log("ReceiveFile SenderName: ", senderName);
        const isCurrentUser = senderName === $(".chat-window h5").text();
        console.log("ReceiveFile isCurrent:", isCurrentUser);
        const fileClass = isCurrentUser ? "justify-content-start" : "justify-content-end";
        const bgClass = isCurrentUser ? "bg-light" : "bg-primary text-white";

        // Dosya türüne göre uygun HTML içeriği oluştur
        let fileHtml = '';
        if (fileUrl.endsWith('.jpg') || fileUrl.endsWith('.png') || fileUrl.endsWith('.gif')) {
            // Resim dosyası
            fileHtml = `
            <div class="d-flex ${fileClass} mb-2">
                <a href="${fileUrl}" target="_blank">
                    <img src="${fileUrl}" alt="${fileName}" class="img-fluid rounded" style="max-width: 200px; max-height: 200px;">
                 </a>
            </div>`;

        } else {
            // Diğer dosya türleri
            fileHtml = `
            <div class="d-flex ${fileClass} mb-2">
                <div class="file-preview ${bgClass} p-2 rounded">
                    <a href="${fileUrl}" download="${fileName}">${fileName}</a>
                </div>
            </div>
        `;
        }

        $("#chat-messages").append(fileHtml);
        $("#chat-messages").scrollTop($("#chat-messages")[0].scrollHeight);
    });

    connection.on("ReceiveMessage", (message, senderName) => {
        const currentChatUser = $(".chat-window h5").text();  // Aktif sohbet edilen kullanıcı
        console.log("currentChatUser:", currentChatUser);

        const currentUserName = $("#currentUserName").val();  // Giriş yapmış kullanıcının adı
        console.log("currentUserName:", currentUserName);
        console.log("senderName:", senderName);

        const isCurrentChatUser = senderName === currentChatUser;  // Mesaj gönderen aktif sohbet edilen kullanıcı mı?
        const isSenderCurrentUser = senderName === currentUserName;  // Mesajı gönderen şu anki kullanıcı mı?

        if (isCurrentChatUser || isSenderCurrentUser) {
            // Mesaj aktif sohbet edilen kullanıcıya ya da gönderen kişiye aitse, mesajı pencereye ekle
            const messageClass = isSenderCurrentUser ? "justify-content-end" : "justify-content-start";
            const bgClass = isSenderCurrentUser ? "bg-primary text-white" : "bg-light";

            const messageHtml = `
        <div class="d-flex ${messageClass} mb-2">
            <div class="message ${bgClass} p-2 rounded">${message.content}</div>
        </div>
    `;
            $("#chat-messages").append(messageHtml);
            $("#chat-messages").scrollTop($("#chat-messages")[0].scrollHeight);
        } else {
            // Mesaj aktif olmayan kullanıcıdan geldiyse, kullanıcı listesinde bildirim göster
            updateUnreadMessageCount(senderName);
        }
    });



    function updateUnreadMessageCount(senderName) {
        const userItem = $(`.list-group-item:contains(${senderName})`);
        let badge = userItem.find('.message-count');

        if (badge.length === 0) {
            // Eğer daha önce okunmamış mesaj sayısı eklenmemişse, yeni bir yuvarlak ekle
            badge = $('<span class="badge bg-success rounded-circle message-count">1</span>');
            userItem.append(badge);
        } else {
            // Okunmamış mesaj sayısını artır
            const count = parseInt(badge.text());
            badge.text(count + 1);
        }
    }

    $(document).on("click", "#_clients .list-group-item", function () {
        const userItem = $(this);
        console.log("UserItem clicked:", userItem);  // Tıklanan öğeyi kontrol et
        const badge = userItem.find('.message-count');
        if (badge.length) {
            badge.remove();  // Yeşil yuvarlağı kaldır
        } else {
            console.log("Badge not found");  // Eğer `badge` bulunamazsa, bu satır tetiklenir
        }
    });

    connection.on("LoadChatHistory", (messages) => {
        $("#chat-messages").empty();
        $.each(messages, (index, messageObj) => {
            const isCurrentUser = messageObj.senderUserName === $(".chat-window h5").text();
            const messageClass = isCurrentUser ? "justify-content-start" : "justify-content-end";
            const bgClass = isCurrentUser ? "bg-light" : "bg-primary text-white";

            let messageHtml = '';

            // Dosya ekleri varsa onları ekleyelim ve messageObj.content'i gösterme
            if (messageObj.attachments && messageObj.attachments.length > 0) {
                $.each(messageObj.attachments, (i, attachment) => {
                    if (attachment.fileType.startsWith("image/")) {
                        // Resim dosyaları için img etiketi kullan ve tıklanabilir yap
                        messageHtml += `
                        <div class="d-flex ${messageClass} mb-2">
                            <a href="${attachment.fileUrl}" target="_blank">
                                <img src="${attachment.fileUrl}" alt="${attachment.fileName}" class="img-fluid rounded" style="max-width: 200px; max-height: 200px;">
                            </a>
                        </div>
                    `;
                    } else {
                        // Diğer dosyalar için bir bağlantı ekleyelim
                        messageHtml += `
                        <div class="d-flex ${messageClass} mb-2">
                            <a href="${attachment.fileUrl}" target="_blank" class="text-decoration-none">
                                File: ${attachment.fileName}
                            </a>
                        </div>
                    `;
                    }
                });
            } else {
                // Dosya yoksa sadece mesaj içeriğini göster
                messageHtml += `
                <div class="d-flex ${messageClass} mb-2">
                    <div class="message ${bgClass} p-2 rounded">${messageObj.content}</div>
                </div>`;
            }

            $("#chat-messages").append(messageHtml);
        });

        $("#chat-messages").scrollTop($("#chat-messages")[0].scrollHeight);
    });
});

tinymce.init({
    selector: "#messageArea",
    plugins: "emoticons",
    toolbar: "emoticons",
    toolbar_location: "bottom",
    menubar: false
});