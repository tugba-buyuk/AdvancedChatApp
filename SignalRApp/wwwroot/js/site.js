$(function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5269/chathub")
        .withAutomaticReconnect([1000, 2000, 4000]) // parantez içinde ms cisinden periyotlar belirlenmezse default olarak 0 2 10 30 s aralıklarla istek atar
        .build();

    connection.start();

})