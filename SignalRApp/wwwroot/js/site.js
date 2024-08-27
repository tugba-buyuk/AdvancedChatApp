$(function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5269/chathub")
        .withAutomaticReconnect([1000, 2000, 4000]) // parantez içinde ms cisinden periyotlar belirlenmezse default olarak 0 2 10 30 s aralıklarla istek atar
        .build();

    connection.start();

    $("#btnLogin").on("click", () => {
        const accepting = $("#accepting").val();
        if (accepting == "on")
        {
            connection.invoke("GetNickName").catch(err => console.log(`Ocurred a error while sending nickname: ${err}`));
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
        console.log("GGeldiiiii");
        $(".accepting-area").addClass("d-none");
        $("#exampleModal").addClass("d-none");
        $(".modal-backdrop").addClass("d-none");
        $(".chat-container").removeClass("d-none");   
        $("#_clients").html("");
        $.each(otherUsers, (index, item) => {
            const user = $(".users").first().clone();
            user.removeClass("active");
            user.find("span.fw-bold").text(item.userName);
            console.log("IMMMMAAAGGEEEE:", item.profileImage);
            user.find("img").attr("src", item.profileImage);
            let lastSeenText = formatLastSeen(item.lastLogin);
            user.find("small.text-muted").text(lastSeenText);
            $("#_clients").append(user);
        });
    });

})