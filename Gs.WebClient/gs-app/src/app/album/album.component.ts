import { Component, OnInit, ViewEncapsulation, AfterViewChecked, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Rx';
import { Location, LocationStrategy, PathLocationStrategy } from '@angular/common';

export class Album {
  Id: number;
  Title: string;
}

export class GalleryData {
  Album: Album;
}

@Injectable()
export class AlbumService {
  //album: any;

  constructor(private _http: HttpClient) {
  }

  getGalleryData(albumId: number): Observable<GalleryData> {
    // This allows the component to assign the album to a local variable.
    return this._http.get<GalleryData>(`https://localhost:44312/api/albums/inflated/${albumId}/`);

    // return this._http.get(`http://localhost/dev/gs/api/albums/${albumId}/inflated`) // /album.json
    //   .map((response: Response) => <GalleryData>response.json())
    //   .do(data => console.log('All: ' + JSON.stringify(data)))
    //   .catch(this.handleError);

    // ALT 1: Assign to variable in service. HTML template accesses via {{ albumService.album.Id }}
    //var aPromise = this._http.get('http://localhost/dev/gs/api/albums/1/get') // /album.json
    //    .toPromise()
    //    .then(response => response.json())
    //    .catch(this.handleError);

    //aPromise.then(albumFromServer => {
    //    this.album = albumFromServer;
    //    //this.album = JSON.parse(albumFromServer);
    //    //debugger;
    //    console.log(this.album);
    //});

    // ALT 2: Same as ALT1 but uses map() and does not have error handler
    //var aPromise = this._http.get('http://localhost/dev/gs/api/albums/1/get') // '/album.json'
    //    .map((response: Response) => response.json().data)
    //    .toPromise();
  }

  // private handleError(error: Response) {
  //   console.error(error);
  //   return Observable.throw(error.json().error || 'Server error');
  // }

  //private handleError(error: any): Promise<any> {
  //    console.error('An error occurred', error); // for demo purposes only
  //    return Promise.reject(error.message || error);
  //}
}

@Component({
  selector: 'app-album',
  templateUrl: './album.component.html',
  styleUrls: ['./album.component.css'],
  providers: [AlbumService, Location, { provide: LocationStrategy, useClass: PathLocationStrategy }],
  encapsulation: ViewEncapsulation.None
})
export class AlbumComponent implements OnInit {
  galleryData: GalleryData;
  location: Location;
  errorMessage: string;

  constructor(private albumService: AlbumService, location: Location) {
    this.location = location;
  }

  ngOnInit() {
    //debugger;
    let albumId = Number(this.location.path().split('/').pop());

    if (isNaN(albumId) || albumId === 0)
      albumId = 1;

    this.albumService.getGalleryData(albumId).subscribe(
      gData => {
        console.log(gData);
        this.galleryData = gData;
      },
      error => this.errorMessage = <any>error);


    $(document).ready(() => {
      if (jQuery) {
        console.log('jQuery is loaded and available.');
      } else {
        console.log('jQuery is NOT loaded.');
      }
    });

  }

  ngAfterViewChecked(): void {
    $('.thmb').equalSize(); // Make all thumbnail tags the same width & height
  }

  getGalleryItemUrl(galleryItem: any) {
    if (galleryItem.isAlbum)
      return `http://localhost:4200/album/${galleryItem.id}`;
    else
      return `http://localhost:4200/media/${galleryItem.id}`;
  }

}
