import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, catchError, finalize, switchMap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

export interface AutomationTask {
  id: string;
  taskType: string;
  cronExpression: string;
  isActive: boolean;
  lastRunDate?: string;
  relatedCampaignPostId?: string;
  storeId: string;
  createdAt: string;
}

export interface UpdateAutomationTaskRequest {
  isActive?: boolean;
  cronExpression?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AutomationTasksService {
  private http = inject(HttpClient);
  private readonly API_URL = `${environment.apiUrl}/automation-tasks`;

  // State signals
  private _tasks = signal<AutomationTask[]>([]);
  private _selectedTask = signal<AutomationTask | null>(null);
  private _isLoading = signal(false);
  private _error = signal<string | null>(null);

  // Public readonly signals
  readonly tasks = this._tasks.asReadonly();
  readonly selectedTask = this._selectedTask.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();

  // Computed
  readonly totalTasks = computed(() => this._tasks().length);
  readonly activeTasks = computed(() => this._tasks().filter(t => t.isActive));
  readonly inactiveTasks = computed(() => this._tasks().filter(t => !t.isActive));

  /**
   * Load all automation tasks for current store
   * GET /api/automation-tasks (STORE-SCOPED)
   */
  loadTasks(): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<AutomationTask[]>(`${this.API_URL}`)
      .pipe(
        tap(tasks => this._tasks.set(tasks)),
        catchError(err => {
          this._error.set(err.error?.message || 'Failed to load automation tasks');
          return of([]);
        }),
        finalize(() => this._isLoading.set(false))
      )
      .subscribe();
  }

  /**
   * Update an automation task (enable/disable)
   * PUT /api/automation-tasks/{taskId} (STORE-SCOPED)
   */
  updateTask(taskId: string, data: UpdateAutomationTaskRequest): Observable<AutomationTask> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.put<AutomationTask>(`${this.API_URL}/${taskId}`, data).pipe(
      tap(task => {
        this._tasks.update(tasks => tasks.map(t => t.id === taskId ? task : t));
        this._selectedTask.set(task);
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to update automation task');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Toggle task active status
   */
  toggleTaskActive(taskId: string): Observable<AutomationTask> {
    const task = this._tasks().find(t => t.id === taskId);
    if (!task) {
      return of({} as AutomationTask).pipe(
        switchMap(() => { throw new Error('Task not found'); })
      );
    }
    return this.updateTask(taskId, { isActive: !task.isActive });
  }

  selectTask(task: AutomationTask): void {
    this._selectedTask.set(task);
  }

  clearSelectedTask(): void {
    this._selectedTask.set(null);
  }

  clearError(): void {
    this._error.set(null);
  }
}
