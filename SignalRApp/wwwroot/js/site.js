$(function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5269/chathub")
        .withAutomaticReconnect([1000, 2000, 4000]) // parantez içinde ms cisinden periyotlar belirlenmezse default olarak 0 2 10 30 s aralıklarla istek atar
        .build();

    connection.start();

    $("#btnLogin").on("click", () => {
        const nickName = $("#txtNickName").val();
        connection.invoke("GetNickName", nickName).catch(err => console.log(`Ocurred a error while sending nickname: ${err}`));

        $("#register-btn-group").addClass("d-none");

        $(".chat-container").removeClass("d-none");
    });

    connection.on("clientList", clientList => {
        $("#_clients").html("");
        $.each(clientList, (index, item) => {
            const user = $(".users").first().clone();
            user.removeClass("active");
            user.html(item.nickName);
            $("#_clients").append(user);
        });
    });

})