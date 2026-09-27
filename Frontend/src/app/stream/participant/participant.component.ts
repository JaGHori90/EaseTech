import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { StreamVideoParticipant } from '@stream-io/video-client';
import { CallService } from '../../shared/services/call.service';

@Component({
  selector: 'app-participant',
  imports: [],
  templateUrl: './participant.component.html',
  styleUrl: './participant.component.css'
})
export class ParticipantComponent {
  @ViewChild('videoElement') videoElement!: ElementRef<HTMLVideoElement>;
  @ViewChild('audioElement') audioElement!: ElementRef<HTMLAudioElement>;

  @Input() participant!: StreamVideoParticipant;
  unbindVideoElement: (() => void) | undefined;
  unbindAudioElement: (() => void) | undefined;

  constructor(private streamService: CallService) {}

  ngAfterViewInit(): void {
    this.unbindVideoElement = this.streamService
      .call()
      ?.bindVideoElement(
        this.videoElement.nativeElement,
        this.participant.sessionId,
        'videoTrack'
      );

    this.unbindAudioElement = this.streamService
      .call()
      ?.bindAudioElement(
        this.audioElement.nativeElement,
        this.participant.sessionId
      );
  }

  ngOnDestroy(): void {
    this.unbindVideoElement?.();
    this.unbindAudioElement?.();
  }
}
