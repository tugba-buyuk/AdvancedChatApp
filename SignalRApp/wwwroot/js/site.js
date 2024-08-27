$(function () {
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
    $("#sendBtn").on("click", () => {
        if (activeUser) {
            const receiverName = activeUser.find('.info-username').text();
            const messageContent = $("#messageArea").val();
            connection.invoke("SendMessage", messageContent, receiverName).catch(err => console.log(`Error while sending message: ${err}`));
            $("#messageArea").val(""); // Mesaj kutusunu temizle
        } else {
            alert("Lütfen bir kullanıcı seçin.");
        }
    });

});
