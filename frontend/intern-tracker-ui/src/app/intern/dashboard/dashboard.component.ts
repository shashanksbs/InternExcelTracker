import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { InternService } from '../../core/services/intern.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-intern-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './dashboard.component.html',
})
export class InternDashboardComponent implements OnInit {
  private fb = inject(FormBuilder);
  private internService = inject(InternService);
  private authService = inject(AuthService);
  private toast = inject(ToastService);

  assignments: any[] = [];
  reports: any[] = [];
  user: any;
  editingReportId: number | null = null;
  
  get activeAssignmentCount(): number {
    return this.assignments ? this.assignments.filter(a => a.status !== 'Completed').length : 0;
  }
  
  // Accordion states
  isReportFormOpen = false;
  isHistoryOpen = false;

  // Filter state
  selectedDate: string = '';

  get visibleReports(): any[] {
    if (!this.selectedDate) {
      return this.reports;
    }

    // Filter by date
    const filtered = this.reports.filter(report => {
      if (!report.createdAt) return false;
      const reportDate = new Date(report.createdAt).toISOString().split('T')[0];
      return reportDate === this.selectedDate;
    });

    // Sort by ID ascending (1 to n)
    return filtered.sort((a, b) => {
        const idA = parseInt(a.productId) || 0;
        const idB = parseInt(b.productId) || 0;
        return idA - idB;
    });
  }

  toggleReportForm() {
    this.isReportFormOpen = !this.isReportFormOpen;
  }

  toggleHistory() {
    this.isHistoryOpen = !this.isHistoryOpen;
  }

  reportForm = this.fb.group({
    assignmentId: [null, Validators.required],
    productName: ['', [Validators.required, Validators.pattern('^[a-zA-Z0-9 ]*$')]],
    imagesCollected: [0, [Validators.required, Validators.min(0)]],
    imageQuality: ['High', Validators.required],
    videoCollected: [false],
    remarks: ['']
  });

  ngOnInit() {
    this.user = this.authService.getUser();
    if (this.user) {
      this.loadAssignments();
      this.loadReports();
    }
  }

  loadAssignments() {
    this.internService.getAssignments(this.user.username).subscribe({
      next: (data) => this.assignments = data,
      error: (err) => console.error(err)
    });
  }

  loadReports() {
    this.internService.getReports(this.user.username).subscribe({
      next: (data) => this.reports = data,
      error: (err) => console.error(err)
    });
  }

  downloadAssignment(id: number, fileName: string) {
    this.internService.downloadAssignment(id).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName; 
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }

  markAsCompleted(assignmentId: number) {
    if(confirm('Mark this assignment as completed?')) {
        this.internService.completeAssignment(assignmentId).subscribe({
            next: (res) => {
                this.toast.success(res.message || 'Assignment completed');
                this.loadAssignments();
            },
            error: (err) => {
                this.toast.error(err.error?.message || 'Failed to mark as completed');
            }
        });
    }
  }

  onSubmitReport() {
    if (this.reportForm.valid) {
      const data = {
        ...this.reportForm.value,
        username: this.user.username
      };

      if (this.editingReportId) {
        this.internService.editReport(this.editingReportId, data).subscribe({
            next: (res) => {
                this.toast.success(res.message || 'Report updated successfully');
                this.resetForm();
                this.loadReports();
            },
            error: (err) => {
                this.toast.error(err.error?.message || 'Update failed');
            }
        });
      } else {
        this.internService.submitReport(data).subscribe({
            next: (res) => {
            this.toast.success(res.message || 'Report submitted successfully');
            this.resetForm();
            this.loadReports();
            },
            error: (err) => {
            this.toast.error(err.error?.message || 'Submission failed');
            }
        });
      }
    } else {
        this.toast.info('Please fill all required fields');
        this.reportForm.markAllAsTouched();
    }
  }

  editReport(report: any) {
    this.editingReportId = report.id;
    this.reportForm.patchValue({
        assignmentId: report.excelAssignmentId,
        productName: report.productName,
        imagesCollected: report.imagesCollected,
        imageQuality: report.imageQuality,
        videoCollected: report.videoCollected,
        remarks: report.remarks
    });
    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancelEdit() {
    this.resetForm();
  }

  resetForm() {
    this.editingReportId = null;
    this.reportForm.reset({ 
        imageQuality: 'High', 
        videoCollected: false, 
        imagesCollected: 0,
        remarks: '',
        productName: ''
    });
  }

  deleteReport(id: number) {
    if (confirm('Are you sure you want to delete this report?')) {
      this.internService.deleteReport(id).subscribe({
        next: () => {
            this.toast.success('Report deleted');
            this.loadReports();
        },
        error: (err) => this.toast.error('Failed to delete: ' + (err.error?.message || 'Unknown error'))
      });
    }
  }
  
  logout() {
    this.authService.logout();
  }
}
