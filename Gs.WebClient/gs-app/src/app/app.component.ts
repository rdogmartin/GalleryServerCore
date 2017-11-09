import { Component } from '@angular/core';
import { OnInit } from '@angular/core/src/metadata/lifecycle_hooks';
import * as $ from 'jquery'

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  ngOnInit(): void {
    $(document).ready(() => {
      if ($) {
          console.log('jQuery is loaded and available.');
      } else {
          console.log('jQuery is NOT loaded.');
      }
  });
  }
  title = 'Gallery Server';
}
