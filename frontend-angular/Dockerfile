# Use an official Node.js runtime as a parent image
# Current LTS is v22
FROM node:22.21-alpine3.21

# Set the working directory in the container
WORKDIR /app

# Copy package.json and package-lock.json
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy the rest of the application's source code
COPY . .

# Build the Angular app for production
RUN npm run build

# Install serve to run the application
RUN npm install -g serve

# Expose the port the app runs on
EXPOSE 4200

# Command to run the app using serve
CMD ["serve", "-s", "dist/frontend-angular", "-l", "4200"]