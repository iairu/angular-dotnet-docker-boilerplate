import { Component, OnInit } from '@angular/core';
import axios, { AxiosResponse } from 'axios';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'Angular + .NET + Docker Boilerplate';
  message: string = '';
  loading: boolean = true;
  error: string = '';

  ngOnInit(): void {
    this.fetchMessage();
  }

  private fetchMessage(): void {
    this.loading = true;
    this.error = '';
    
    // Fetch JSON data from the backend with .json appendix for compatibility with static Github Pages deployment
    // (if ran on live server, live API access still works this way)
    axios.get('/api/hello.json')
      .then((response: AxiosResponse<string>) => {
        this.message = response.data;
        this.loading = false;
      })
      .catch((error: any) => {
        console.error('There was an error fetching the data!', error);
        this.error = 'Failed to fetch data from the backend. Please check if the server is running.';
        this.loading = false;
      });
  }

  retryFetch(): void {
    this.fetchMessage();
  }
}