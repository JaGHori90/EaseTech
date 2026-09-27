import { Component, inject, Input, Signal } from '@angular/core';
import { Call, StreamVideoParticipant } from '@stream-io/video-client';
import { CallService } from '../../shared/services/call.service';
import { ParticipantComponent } from '../participant/participant.component';
import { CommonModule } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';


@Component({
  selector: 'app-call',
  imports: [CommonModule, ParticipantComponent],
  templateUrl: './call.component.html',
  styleUrl: './call.component.css'
})
export class CallComponent {

  private router= inject(Router);

  @Input({ required: true }) call!: Call;
  

  participants: Signal<StreamVideoParticipant[]>;

  constructor(private streamService: CallService) {
    this.participants = toSignal(
      this.streamService.call()!.state.participants$,
      { requireSync: true }
    );
  }

  toggleMicrophone() {
    this.call.microphone.toggle();
  }

  toggleCamera() {
    this.call.camera.toggle();
  }

  trackBySessionId(_: number, participant: StreamVideoParticipant) {
    return participant.sessionId;
  }

  leaveCall() {
    this.streamService.setCallId(undefined);
    localStorage.removeItem('callData');

    setTimeout(() => {
      this.router.navigateByUrl('/page/home');
    }, 5000);
  }

  
}


