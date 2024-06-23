import { ErrorResponse } from "./models/errorresponse.js";
import { Response } from "./models/response.js";

const locBox = document.querySelector(".loc");
const catBox = document.querySelector(".cat");
const button = document.querySelector(".submit");

const PORT = 1738

button.onclick = async() => {
    const address = `http://localhost:${PORT}/?location=${locBox.value}&categories=${catBox.value}`;
    console.log(`Sending request to ${address}`);
    const result = await fetch(address).then(res => {
                                                if (!res.ok) {
                                                    return res.json().then(errorData => {
                                                        throw new Error(`Error: ${errorData.Message}`);
                                                    });
                                                }
                                                return res.json();
                                        })
                                        .then(data => {
                                            const response = new Response(data.businessCount, data.businesses, data.timeTaken);
                                            response.drawResponse(document.querySelector(".responses"));
                                        })
                                        .catch(err => {
                                            console.log(err);
                                            const errorResponse = new ErrorResponse(err.message);
                                            errorResponse.drawResponse(document.querySelector(".responses"));
                                        });
                                        console.log(result);
}