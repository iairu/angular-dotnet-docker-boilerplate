# Angular Frontend

This is the Angular frontend application for the Angular + .NET + Docker boilerplate.

## 🚀 Features

- **Angular 18.x**: Modern Angular framework with TypeScript
- **Axios Integration**: HTTP client for API communication with .NET backend
- **Responsive Design**: Mobile-first responsive layout
- **Docker Ready**: Containerized for consistent deployment
- **Production Optimized**: Built with Angular CLI optimizations

## 📋 Prerequisites

- Node.js 22.x or higher
- npm or yarn
- Angular CLI (optional, for development)

## 🛠 Development

### Install Dependencies

```bash
npm install
```

### Start Development Server

```bash
npm run dev
# or
npm run start
```

The application will be available at `http://localhost:4200`

### Build for Production

```bash
npm run build
```

Built files will be in the `dist/frontend-angular` directory.

## 🐳 Docker Usage

### Build Docker Image

```bash
docker build -t angular-frontend .
```

### Run Docker Container

```bash
docker run -p 4200:4200 angular-frontend
```

## 📁 Project Structure

```
src/
├── app/
│   ├── app.component.ts       # Main app component with API integration
│   ├── app.component.html     # App template
│   ├── app.component.css      # App styles
│   ├── app.module.ts          # Root module
│   └── app-routing.module.ts  # Routing configuration
├── assets/                    # Static assets
├── index.html                 # Main HTML template
├── main.ts                    # Application bootstrap
└── styles.css                 # Global styles
```

## 🔌 API Integration

The frontend uses Axios to communicate with the .NET backend:

```typescript
// Example API call
import axios from 'axios';

axios.get('/api/hello.json')
  .then(response => {
    console.log(response.data);
  })
  .catch(error => {
    console.error('API Error:', error);
  });
```

## 🎨 Styling

- Global styles in `src/styles.css`
- Component-specific styles in respective `.css` files
- Utility classes for responsive design
- Dark/light theme support via CSS variables

## 📝 Available Scripts

- `npm run start` - Start development server
- `npm run build` - Build for production
- `npm run test` - Run unit tests
- `npm run lint` - Run linter

## 🔧 Configuration

### Environment Variables

The application can be configured using environment variables:

- `NG_APP_API_URL` - Backend API URL (default: `/api`)
- `NODE_ENV` - Environment mode (development/production)

### Angular Configuration

Configuration files:
- `angular.json` - Angular workspace configuration
- `tsconfig.json` - TypeScript configuration
- `package.json` - Dependencies and scripts

## 🐛 Troubleshooting

### Common Issues

**API calls fail:**
- Ensure the backend is running
- Check network connectivity
- Verify API endpoints are correct

**Build fails:**
- Clear `node_modules` and reinstall: `rm -rf node_modules && npm install`
- Check TypeScript errors: `npm run build`

**Development server issues:**
- Try different port: `ng serve --port 4201`
- Clear Angular cache: `rm -rf .angular/cache`

## 📦 Technologies Used

- **Angular 18.x** - Frontend framework
- **TypeScript** - Type-safe JavaScript
- **Axios** - HTTP client library
- **RxJS** - Reactive programming
- **Zone.js** - Change detection

---

Part of the [Angular + .NET + Docker Boilerplate](https://github.com/iairu/angular-dotnet-docker-boilerplate)