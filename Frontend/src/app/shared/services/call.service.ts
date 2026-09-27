import { HttpClient, HttpHeaders } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Call, StreamVideoClient, User } from '@stream-io/video-client';
import { UserService } from './user.service';
import { AuthService } from './auth.service';
import { CallApiService } from './call-api.service';

@Injectable({
  providedIn: 'root'
})
export class CallService {

  callId = signal<string | undefined>(undefined);
  client: StreamVideoClient | undefined;

  constructor() {
    const callData = this.getStoredCallData();

    if (callData) {
      this.setClient(callData); // Wenn callData existiert, setze den Client
    }
  }

  // Holt die Call-Daten aus dem localStorage
  getStoredCallData() {
    const storedData = localStorage.getItem('callData');
    return storedData ? JSON.parse(storedData) : null;
  }

  // Setzt die Call-Daten und initialisiert den StreamVideoClient
  setStoredCallData(callData: any) {
    localStorage.setItem('callData', JSON.stringify(callData)); // Speichert die Call-Daten im localStorage
    this.setClient(callData); // Initialisiert den Client mit den Call-Daten
  }

  // Setzt den StreamVideoClient mit den Call-Daten
  setClient(callData: any) {
    const apiKey = callData.apiKey; // kommt vom Backend (/CreateToken), nicht mehr fest im Code
    const token = callData.token;
    const user: User = { id: callData.userId };

    // Initialisiert den Client mit den Call-Daten
    this.client = new StreamVideoClient({ apiKey, token, user });
    this.callId.set(callData.callId); // Setzt die callId in den signal
  }

  // Berechnet den Call aus den Client-Daten
  call = computed<Call | undefined>(() => {
    const currentCallId = this.callId();
    if (currentCallId !== undefined && this.client) {
      const call = this.client.call('default', currentCallId); // Holt den Call mit der aktuellen CallId

      // Treten dem Call bei und aktivieren Kamera/Mikrofon
      call.join({ create: true }).then(async () => {
        call.camera.enable();
        call.microphone.enable();
      });
      return call;
    } else {
      return undefined;
    }
  });

  // Startet den Call
  startCall() {
    const callInstance = this.call();
    if (callInstance) {
      callInstance.join({ create: true }).then(async () => {
        callInstance.camera.enable();
        callInstance.microphone.enable();
      });
    }
  }

  // Setzt die CallId (kann auch als Update verwendet werden)
  setCallId(callId: string | undefined) {
    if (callId === undefined) {
      this.call()?.leave(); // Verlasse den Call, wenn callId undefined ist
    }
    this.callId.set(callId); // Setze die CallId
  }
}
