import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/admin`;

  constructor() { }

  uploadExcel(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/upload-excel`, formData);
  }

  getReports(username: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/reports?username=${username}`);
  }

  approveReport(reportId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/approve/${reportId}`, {});
  }

  rejectReport(reportId: number, reason: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/reject/${reportId}`, { reason });
  }

  getPerformance(username: string, date?: string): Observable<any> {
    let url = `${this.apiUrl}/performance?username=${username}`;
    if (date) {
      url += `&date=${date}`;
    }
    return this.http.get<any>(url);
  }
  
  getAllInterns(): Observable<any[]> {
      return this.http.get<any[]>(`${this.apiUrl}/interns`);
  }
}
