
function connectToSignalR() {
	var conn = new signalR.HubConnectionBuilder().withUrl("/hub").build();
	conn.on("HeyANewCarIsForSale", showNotification);
	conn.start().then(function () {
		console.log("Connected to SignalR! 🎉");
	}).catch(function (err) {
		console.error("Error connecting to SignalR: 🤣", err);
	});
}

function showNotification(user, message) {
	console.log(user);
	console.log(message);
	var data = JSON.parse(message);
	const div = document.createElement("div");
	div.classList.add("show");
	div.innerHTML = `
	${data.Make} ${data.Model} (${data.Year}, ${data.Color}) <br />
	PRICE: ${data.Price} ${data.CurrencyCode}<br />
	<a href="/vehicles/details/${data.Registration}">click for more!</a>
	`;
	target.prepend(div);
	div.style.setProperty("--background-color", data.Color);
	window.setTimeout(() => div.classList.replace("show", "hide"), 4000);
	window.setTimeout(() => div.remove(), 5000);
}
connectToSignalR();
const target = document.getElementById('signalr-notifications');
