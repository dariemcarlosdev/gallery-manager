import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';

// Bootstraps the standalone root component with app-wide providers.
bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));
