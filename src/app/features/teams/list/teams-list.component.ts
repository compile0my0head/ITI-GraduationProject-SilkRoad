import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-teams-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './teams-list.component.html',
  styleUrls: ['./teams-list.component.scss']
})
export class TeamsListComponent {
  // Teams feature is coming soon - placeholder only
}
