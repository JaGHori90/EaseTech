import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { StreamVideoParticipant } from '@stream-io/video-client';

import { ParticipantComponent } from './participant.component';
import { CallService } from '../../shared/services/call.service';

describe('ParticipantComponent', () => {
  let component: ParticipantComponent;
  let fixture: ComponentFixture<ParticipantComponent>;
  let unbindVideo: jasmine.Spy;
  let unbindAudio: jasmine.Spy;
  let fakeCall: any;

  beforeEach(async () => {
    unbindVideo = jasmine.createSpy('unbindVideo');
    unbindAudio = jasmine.createSpy('unbindAudio');
    fakeCall = {
      bindVideoElement: jasmine.createSpy('bindVideoElement').and.returnValue(unbindVideo),
      bindAudioElement: jasmine.createSpy('bindAudioElement').and.returnValue(unbindAudio),
    };

    await TestBed.configureTestingModule({
      imports: [ParticipantComponent],
      // Echter CallService würde sich mit Stream verbinden → durch Fake ersetzen
      providers: [{ provide: CallService, useValue: { call: signal(fakeCall) } }],
    }).compileComponents();

    fixture = TestBed.createComponent(ParticipantComponent);
    component = fixture.componentInstance;
    component.participant = { sessionId: 'session-1', name: 'Andrea' } as StreamVideoParticipant;
    fixture.detectChanges();
  });

  it('zeigt den Namen des Teilnehmers', () => {
    expect(fixture.nativeElement.textContent).toContain('Andrea');
  });

  it('bindet Video und Audio an die Session', () => {
    expect(fakeCall.bindVideoElement).toHaveBeenCalledWith(jasmine.any(HTMLVideoElement), 'session-1', 'videoTrack');
    expect(fakeCall.bindAudioElement).toHaveBeenCalledWith(jasmine.any(HTMLAudioElement), 'session-1');
  });

  it('löst die Bindungen beim Zerstören', () => {
    fixture.destroy();

    expect(unbindVideo).toHaveBeenCalled();
    expect(unbindAudio).toHaveBeenCalled();
  });
});
