import { Business } from "./business.js"
export class Response{
    constructor(total, businesses, totaltime){
        this.Total = total;
        this.Businesses = [];
        businesses.forEach(business => {
            this.Businesses.push(new Business(business.Name, business.review_count, business.Rating, business.Price));
        });
        this.TotalTime = totaltime;
    }

    drawResponse(container){

        const resultDiv = document.createElement("div");
        resultDiv.classList.add("response");
        const businessesDiv = document.createElement("div");
        
        const totalBusinesses = document.createElement("label");  
        totalBusinesses.innerHTML = this.Total
        totalBusinesses.classList.add("margin-10");
        
        businessesDiv.appendChild(totalBusinesses);
        resultDiv.appendChild(businessesDiv);
        
        this.Businesses.forEach(business => {
            
            let bDiv = document.createElement("div");
            bDiv.classList.add("form-gr");
            
            const name = document.createElement("label");
            name.classList.add("margin-10")
            name.innerHTML = business.Name; 

            const reviewcount = document.createElement("label");
            reviewcount.classList.add("margin-10")
            reviewcount.innerHTML = business.ReviewCount; 
            
            const rating = document.createElement("label");
            rating.classList.add("margin-10")
            rating.innerHTML = business.Rating; 
            
            const price = document.createElement("label");
            price.classList.add("margin-10")
            price.innerHTML = business.Price; 

            bDiv.appendChild(name);
            bDiv.appendChild(reviewcount);
            bDiv.appendChild(rating);
            bDiv.appendChild(price);
            resultDiv.appendChild(bDiv);

        });

        const timeDiv = document.createElement("div");
        timeDiv.classList.add("time-gr");
            
        const totalTime = document.createElement("label");
        totalBusinesses.classList.add("margin-10")
        totalBusinesses.innerHTML = "Time taken: ";
            
        const timeNumber = document.createElement("label");
        timeNumber.classList.add("margin-10")
        timeNumber.innerHTML = this.TotalTime+"ms"; 
        
        timeDiv.appendChild(totalBusinesses);
        timeDiv.appendChild(timeNumber);
        resultDiv.appendChild(timeDiv);

        container.appendChild(resultDiv);

    }
}