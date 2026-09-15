function connectToSignalR() {
	var conn = new signalR.HubConnectionBuilder().withUrl("/hub").build();
	conn.on("HeyANewCarIsForSale", (user, message) => {
		console.log(user);
		console.log(message);
	});
	conn.start().then(function () {
		console.log("Connected to SignalR! 🎉");
	}).catch(function (err) {
		console.error("Error connecting to SignalR: 🤣", err);
	});
}

$(document).ready(connectToSignalR);
