import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubsUrl;
  private hubConnection?: HubConnection;
  private tosatr = inject(ToastrService); 

  createHubConnection(user: User) 
  {
    this.hubConnection = new HubConnectionBuilder()
    .withUrl(this.hubUrl + 'presence', {
      accessTokenFactory: () => user.token
    }).withAutomaticReconnect().build();

    this.hubConnection.start().catch(error => console.log(error));

    this.hubConnection.on('UserIsOnline', username => 
      {
        this.tosatr.info(username + ' has connected');
      });
    this.hubConnection.on('UserIsOffline', username => 
      {
        this.tosatr.warning(username + ' has disconnected');
      });
  }

  stopHubConnection() 
  {
    if(this.hubConnection?.state === HubConnectionState.Connected) 
      {
        this.hubConnection?.stop().catch(error => console.log(error));
      }
  }
}
