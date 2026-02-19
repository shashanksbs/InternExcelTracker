import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './dashboard.component.html',
})
export class AdminDashboardComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminService = inject(AdminService);
  private authService = inject(AuthService);
  private toast = inject(ToastService);

  interns: any[] = [];
  reports: any[] = [];
  stats: any = null;
  selectedInternUsername = '';
  user: any;
  
  // Filter state
  selectedDate: string = '';
  selectedAssignment: string = '';

  get uniqueAssignments(): string[] {
    if (!this.reports) return [];
    const assignmentNames = this.reports
      .map(r => r.excelAssignment?.fileName)
      .filter((name): name is string => !!name); 
    return [...new Set(assignmentNames)].sort();
  }

  get visibleReports(): any[] {
    let filtered = this.reports;

    // Filter by Date
    if (this.selectedDate) {
      filtered = filtered.filter(report => {
        if (!report.createdAt) return false;
        const reportDate = new Date(report.createdAt).toISOString().split('T')[0];
        return reportDate === this.selectedDate;
      });
    }

    // Filter by Assignment
    if (this.selectedAssignment) {
      filtered = filtered.filter(report => 
        report.excelAssignment?.fileName === this.selectedAssignment
      );
    }

    // Sort by ID ascending (1 to n)
    return filtered.sort((a, b) => {
        const idA = parseInt(a.productId) || 0;
        const idB = parseInt(b.productId) || 0;
        return idA - idB;
    });
  }

  get calculatedStats() {
    const reports = this.visibleReports;
    return {
      totalProducts: reports.length,
      approved: reports.filter(r => r.approvalStatus === 'Approved').length,
      pending: reports.filter(r => r.approvalStatus === 'Pending').length,
      rejected: reports.filter(r => r.approvalStatus === 'Rejected').length
    };
  }
  
  uploadForm = this.fb.group({
    file: [null, Validators.required],
    assignedTo: ['', Validators.required]
  });

  get currentFile(): File | null {
    return this.uploadForm.get('file')?.value as File | null;
  }

  ngOnInit() {
    this.user = this.authService.getUser();
    this.loadInterns();
  }

  loadInterns() {
    this.adminService.getAllInterns().subscribe(data => {
      this.interns = data;
    });
  }

  onFileChange(event: any) {
    if (event.target.files.length > 0) {
      const file = event.target.files[0];
      this.uploadForm.patchValue({
        file: file
      });
    }
  }

  onUpload() {
    if (this.uploadForm.valid) {
      const formData = new FormData();
      formData.append('file', this.uploadForm.get('file')?.value!);
      formData.append('assignedToUsername', this.uploadForm.get('assignedTo')?.value!);
      formData.append('uploadedByUsername', this.authService.getUser().username);

      this.adminService.uploadExcel(formData).subscribe({
        next: (res) => {
          this.toast.success(res.message || 'Excel assigned successfully');
          this.uploadForm.reset();
        },
        error: (err) => {
          this.toast.error(err.error?.message || 'Upload failed');
        }
      });
    }
  }

  onSelectIntern(username: string) {
    this.selectedInternUsername = username;
    if (username) {
        this.loadReports(username);
        this.loadStats(username);
    } else {
        this.reports = [];
        this.stats = null;
    }
  }

  loadReports(username: string) {
    this.adminService.getReports(username).subscribe(data => {
      this.reports = data;
    });
  }

  loadStats(username: string) {
    this.adminService.getPerformance(username).subscribe(data => {
      this.stats = data;
    });
  }

  approveReport(reportId: number) {
    this.adminService.approveReport(reportId).subscribe(() => {
      this.toast.success('Report approved');
      this.loadReports(this.selectedInternUsername);
      this.loadStats(this.selectedInternUsername);
    });
  }

  rejectingReportId: number | null = null;
  rejectionReason: string = '';

  isSidebarOpen = false;

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  initiateReject(reportId: number) {
    this.rejectingReportId = reportId;
    this.rejectionReason = '';
  }

  cancelReject() {
    this.rejectingReportId = null;
    this.rejectionReason = '';
  }

  confirmReject(reportId: number) {
    if (!this.rejectionReason.trim()) {
      this.toast.error('Please enter a reason');
      return;
    }
    
    this.adminService.rejectReport(reportId, this.rejectionReason).subscribe(() => {
      this.toast.info('Report rejected');
      this.loadReports(this.selectedInternUsername);
      this.loadStats(this.selectedInternUsername);
      this.cancelReject();
    });
  }
  
  logout() {
    this.authService.logout();
  }
}
