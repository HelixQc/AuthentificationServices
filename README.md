# 🌟 **GABY LANGIS**

Welcome to **My project**! Follow the steps below to get your environment up and running smoothly. 🚀✨

---
## 🛠️ **Prerequisites**

Before you begin, make sure you have the following installed on your system:

- 🖥️ **Code Editor**: Visual Studio or Rider  
- 🗂️ **Version Control**: Git  
- 🐳 **Containerization**: Docker  
- 📊 **Database Management**: pgAdmin4 or any preferred data grid tool
- 🛠️ **Tools**: Git Bash
---

## 🚀 **Getting Started**

### **Step 1: 🗂️ Create the Folder Structure** (if not done already)

1. 🏗️ Create a folder named `kshtech`.  
2. 📁 Inside the `kshtech` folder, create another folder named `kshtech-db`.  
3. 🔄 Clone the repository **kshtech_API** into the `kshtech` folder using the command:

    ```sh
    https://github.com/HelixQc/AuthentificationServices.git
    ```  

---

### **Step 2: 🐘 Set Up the Database**

1. 🛳️ Use the following Docker command to create a PostgreSQL database container:

    ```sh
    docker run --name <Your Container Name> -e POSTGRES_USER=<Your Postgres Username> -e POSTGRES_PASSWORD=<Your Password> -e POSTGRES_DB=<Your Database Name> -p <Port Number>:5432 -v <Your File Path>:/var/lib/postgresql/data postgres  
    ```  

   📝 **Explanation of the command:**  
   - 🔹 `<Your Container Name>`: Replace with your preferred name for the Docker container.  
   - 🔹 `<Your Postgres Username>`: Specify your PostgreSQL username.  
   - 🔹 `<Your Password>`: Set your PostgreSQL password.  
   - 🔹 `<Your Database Name>`: Name the database as desired.  
   - 🔹 `<Port Number>`: Choose the port you want to use (e.g., `5432`).  
   - 🔹 `<Your File Path>`: Provide the path to the `kshtech-db` folder you created earlier.  

   💡 **Note:** This command initializes a PostgreSQL database inside a Docker container.
> [!IMPORTANT]
> Verify that you're using the absolute path to your folder.
---
### **Step 3: 🛠️ Create the `appsettings.json` File**

1. Create a `appsettings.json` file
2. Follow the template: 
```json
  {
    "ConnectionStrings": {
      "PostgresSQLConnection": "Server=localhost;Database=<Your Database Name>;port=<Port Number>;User id=<Your Postgres Username>;password=<Your Password>"
    },
    "Logging": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "AllowedHosts": "*",
    "Jwt": {
      "Key": "Generate a random key with Git Bash",
      "Issuer": "http://localhost:<API port>",
      "Audience": "http://localhost:<API port>"
    },
    "EmailConfiguration": {
      "From": "dev Email",
      "SmtpServer": "smtp.gmail.com",
      "Port": 465,
      "Username": "dev Email",
      "Password": "dev password"
    },
    "Url": "http://localhost:<the fetching port>",
    "IsDevelopment": true
  }
  ```
3. Fill out the file with project-specific information as provided by the project manager.  
4. To generate a secure key for authentication, use the following command in **Git Bash**:

    ```bash
    openssl rand -base64 32
    ```  

   💡 **Note:** The generated secret key will be used for authentication in your project.

---

### **Step 4: 📊 Update the Database**

1. Open the project in your preferred IDE (e.g., **Visual Studio** or **Rider**).  
2. In the **Package Manager Console**, run the following command:

    ```
    update-database
    ```  

   💡 **Note:** This command initializes the database with Identity Framework tables and other schema updates.

---

### **Step 5: 🚀 Start the API**

1. Launch the API project in your IDE.  
2. Start the API server.  
3. Use the API in future projects or integrate it with other services.

---

## Enjoy developing with **My API**! 💻✨🚀  

---

## 🛡️ License

This project is licensed under the **MIT License**. By using, modifying, or distributing the code, you agree to the following terms:

1. 🏷️ **Ownership**: All intellectual property rights, including but not limited to the source code, design, and architecture, are owned by **Gaby Langis**.  
2. ❌ **Restrictions**:  
   - You may use, copy, modify, and distribute the software freely, but don’t forget to include the copyright notice.  
   - Don’t hold **ME** responsible for anything that goes wrong. Use at your own risk!  
3. 🔐 **Usage**:  
   - Feel free to use the software for personal, educational, or even commercial purposes—just play nice and respect the terms.  
4. ⚠️ **Liability**:  
   - **Gaby Langis** is not liable for any damages, issues, or bad vibes that arise from the use or misuse of this software. Use it wisely! 📄

---

### 🥳 Thank you for using **My Softwares**!  
Happy coding! 💻🚀
