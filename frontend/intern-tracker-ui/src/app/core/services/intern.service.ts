import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class InternService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/intern`;

  constructor() { }

  getAssignments(username: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/assignments/${username}`);
  }

  downloadAssignment(assignmentId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/download/${assignmentId}`, { responseType: 'blob' });
  }

  submitReport(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/submit-report`, data);
  }

  getReports(username: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/reports/${username}`);
  }

  editReport(reportId: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/edit-report/${reportId}`, data);
  }

  deleteReport(reportId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/delete-report/${reportId}`);
  }

  completeAssignment(assignmentId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/complete-assignment/${assignmentId}`, {});
  }
}
