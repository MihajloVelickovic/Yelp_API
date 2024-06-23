const locBox = document.querySelector(".loc");
const catBox = document.querySelector(".cat");
const button = document.querySelector(".submit");

const PORT = 1738

button.onclick = async() => {
    const address = `http://localhost:${PORT}/?location=${locBox.value}&categories=${catBox.value}`;
    console.log(`Sending request to ${address}`);
    const result = await fetch(address).then(res => res.json());
    console.log(result);
}