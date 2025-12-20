import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, catchError, finalize } from 'rxjs/operators';
import { Team } from '../../../core/models/api.models';
import { environment } from '../../../../environments/environment';

export interface TeamMember {
  teamId: string;
  userId: string;
  userName: string;
  userEmail: string;
  role: 'Owner' | 'Moderator' | 'Member';
  joinedAt: string;
}

export interface CreateTeamRequest {
  teamName: string;
  description?: string;
}

export interface InviteMemberRequest {
  userEmail: string;
  role: 'Moderator' | 'Member';
}

@Injectable({
  providedIn: 'root'
})
export class TeamsService {
  private http = inject(HttpClient);
  private readonly API_URL = `${environment.apiUrl}/teams`;

  // State signals
  private _teams = signal<Team[]>([]);
  private _selectedTeam = signal<Team | null>(null);
  private _teamMembers = signal<TeamMember[]>([]);
  private _isLoading = signal(false);
  private _error = signal<string | null>(null);

  // Public readonly signals
  readonly teams = this._teams.asReadonly();
  readonly selectedTeam = this._selectedTeam.asReadonly();
  readonly teamMembers = this._teamMembers.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();

  // Computed
  readonly totalTeams = computed(() => this._teams().length);

  /**
   * Load all teams for current store
   * GET /api/teams (STORE-SCOPED)
   */
  loadTeams(): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<Team[]>(`${this.API_URL}`)
      .pipe(
        tap(teams => this._teams.set(teams)),
        catchError(err => {
          this._error.set(err.error?.message || 'Failed to load teams');
          return of([]);
        }),
        finalize(() => this._isLoading.set(false))
      )
      .subscribe();
  }

  /**
   * Create a new team
   * POST /api/teams (STORE-SCOPED)
   */
  createTeam(data: CreateTeamRequest): Observable<Team> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.post<Team>(`${this.API_URL}`, data).pipe(
      tap(team => this._teams.update(teams => [...teams, team])),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to create team');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Load team members
   * GET /api/teams/{teamId}/members (STORE-SCOPED)
   */
  loadTeamMembers(teamId: string): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<TeamMember[]>(`${this.API_URL}/${teamId}/members`)
      .pipe(
        tap(members => this._teamMembers.set(members)),
        catchError(err => {
          this._error.set(err.error?.message || 'Failed to load team members');
          return of([]);
        }),
        finalize(() => this._isLoading.set(false))
      )
      .subscribe();
  }

  /**
   * Invite a member to team
   * POST /api/teams/{teamId}/members (STORE-SCOPED)
   */
  inviteMember(teamId: string, data: InviteMemberRequest): Observable<TeamMember> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.post<TeamMember>(`${this.API_URL}/${teamId}/members`, data).pipe(
      tap(member => {
        this._teamMembers.update(members => [...members, member]);
        // Update member count in team
        this._teams.update(teams => teams.map(t => t.id === teamId
          ? { ...t, memberCount: t.memberCount + 1 }
          : t
        ));
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to invite member');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Remove a member from team
   * DELETE /api/teams/{teamId}/members/{userId} (STORE-SCOPED)
   */
  removeMember(teamId: string, userId: string): Observable<void> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.delete<void>(`${this.API_URL}/${teamId}/members/${userId}`).pipe(
      tap(() => {
        this._teamMembers.update(members => members.filter(m => m.userId !== userId));
        // Update member count in team
        this._teams.update(teams => teams.map(t => t.id === teamId
          ? { ...t, memberCount: Math.max(0, t.memberCount - 1) }
          : t
        ));
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to remove member');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  selectTeam(team: Team): void {
    this._selectedTeam.set(team);
  }

  clearSelectedTeam(): void {
    this._selectedTeam.set(null);
    this._teamMembers.set([]);
  }

  clearError(): void {
    this._error.set(null);
  }
}
