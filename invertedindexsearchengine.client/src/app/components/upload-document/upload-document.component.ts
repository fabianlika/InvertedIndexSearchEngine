import { Component } from '@angular/core';
import { DocumentService } from '../../services/document.service';

@Component({
  selector: 'app-upload-document',
  templateUrl: './upload-document.component.html',
  styleUrls: ['./upload-document.component.css']
})
export class UploadDocumentComponent {
  title = '';
  content = '';
  message = '';
  selectedFile: File | null = null;
  isDragOver = false;
  isLoading = false;
  uploadMode: 'file' | 'manual' = 'file';
  fileUploadStep: 1 | 2 = 1;

  supportedFormats = ['PDF', 'DOCX', 'TXT'];

  constructor(private documentService: DocumentService) {}

  // Drag events
  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFileSelection(files[0]);
    }
  }

  onFileInputChange(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFileSelection(input.files[0]);
    }
  }

  handleFileSelection(file: File) {
    const allowedTypes = [
      'application/pdf',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'text/plain'
    ];

    const fileExtension = file.name.split('.').pop()?.toUpperCase();

    if (!allowedTypes.includes(file.type) && fileExtension !== 'DOCX') {
      this.message = '❌ Unsupported file type. Please upload PDF, DOCX, or TXT files.';
      return;
    }

    this.selectedFile = file;
    this.title = file.name.replace(/\.[^/.]+$/, '');
    this.message = `✓ File selected: ${file.name}`;
    this.fileUploadStep = 2;

    // Auto-read TXT files only for preview
    if (file.type === 'text/plain') {
      this.readTextFile(file);
    }
  }

  readTextFile(file: File) {
    const reader = new FileReader();
    reader.onload = (e) => {
      this.content = e.target?.result as string;
    };
    reader.onerror = () => {
      this.message = '❌ Error reading file';
    };
    reader.readAsText(file);
  }

 uploadFile() {
  if (!this.selectedFile) {
    this.message = '❌ Please select a file';
    return;
  }

  if (!this.title.trim()) {
    this.message = '❌ Please provide a document title';
    return;
  }

  this.isLoading = true;

  this.documentService.uploadDocumentFile(this.title, this.selectedFile)
    .subscribe({
      next: res => {
        // Show success message
        this.message = `✓ ${res}`;
        this.isLoading = false;

        // Clear file after 2 seconds so user sees message
        setTimeout(() => this.clearFile(), 2000);
      },
      error: err => {
        console.error(err);
        this.message = '❌ Error uploading document';
        this.isLoading = false;
      }
    });
}


  // MANUAL upload
  uploadManual() {
    if (!this.title.trim() || !this.content.trim()) {
      this.message = '❌ Title and content are required!';
      return;
    }

    this.isLoading = true;

    this.documentService.uploadDocument(this.title, this.content)
      .subscribe({
        next: res => {
          this.message = `✓ ${res}`;
          this.title = '';
          this.content = '';
          this.isLoading = false;
        },
        error: err => {
          console.error(err);
          this.message = '❌ Error uploading document';
          this.isLoading = false;
        }
      });
  }

  // MAIN upload switch
  upload() {
    if (this.uploadMode === 'file' && this.selectedFile) {
      this.uploadFile();
    } 
    else if (this.uploadMode === 'manual') {
      this.uploadManual();
    } 
    else {
      this.message = '❌ Please select a file or enter content manually';
    }
  }

  // Seed DB
  seed() {
    this.isLoading = true;

    this.documentService.seedDocuments()
      .subscribe({
        next: res => {
          this.message = `✓ ${res}`;
          this.isLoading = false;
        },
        error: err => {
          console.error(err);
          this.message = '❌ Error seeding documents';
          this.isLoading = false;
        }
      });
  }

  clearFile() {
    this.selectedFile = null;
    this.title = '';
    this.content = '';
    this.message = '';
    this.fileUploadStep = 1;
  }
}
