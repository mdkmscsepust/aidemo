import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { AfterViewChecked, Component, ElementRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements AfterViewChecked {
  private static readonly storageKey = 'chat-api-base-url';

  @ViewChild('scrollAnchor') scrollAnchor?: ElementRef<HTMLDivElement>;
  @ViewChild('messagesContainer') messagesContainer?: ElementRef<HTMLDivElement>;

  apiBaseUrl = localStorage.getItem(AppComponent.storageKey) ?? 'http://127.0.0.1:5050';
  useStreaming = true;
  activePanel: 'chat' | 'cv' = 'chat';
  message = '';
  temperature = 0.7;
  maxTokens = 256;
  isSending = false;
  errorMessage = '';

  cvText = '';
  cvFile: File | null = null;
  cvFileName = '';
  cvIsChecking = false;
  cvError = '';
  cvResult: CvReview | null = null;

  messages: ChatMessage[] = [
    { role: 'assistant', content: 'Hi, I am your local Ollama assistant. Ask me anything.' }
  ];

  private shouldScroll = false;
  private isStreaming = false;
  private streamingAssistantIndex = -1;

  constructor(private readonly http: HttpClient) {}

  ngAfterViewChecked(): void {
    if (!this.shouldScroll || !this.scrollAnchor) {
      return;
    }

    const container = this.messagesContainer?.nativeElement;
    if (container && !this.isNearBottom(container)) {
      this.shouldScroll = false;
      return;
    }

    const behavior: ScrollBehavior = this.isStreaming ? 'auto' : 'smooth';
    this.scrollAnchor.nativeElement.scrollIntoView({ behavior });
    this.shouldScroll = false;
  }

  persistApiBaseUrl(): void {
    localStorage.setItem(AppComponent.storageKey, this.apiBaseUrl.trim());
  }

  async sendMessage(): Promise<void> {
    const content = this.message.trim();
    if (!content || this.isSending) {
      return;
    }

    this.errorMessage = '';
    this.isSending = true;
    this.messages = [...this.messages, { role: 'user', content }];
    this.message = '';
    this.shouldScroll = true;

    const payload: ChatRequest = {
      message: content,
      temperature: this.temperature,
      maxTokens: this.maxTokens
    };

    const baseUrl = this.apiBaseUrl.trim();
    const url = baseUrl ? `${baseUrl}/api/chat` : '/api/chat';

    if (this.useStreaming) {
      await this.sendStreamingMessage(url.replace('/api/chat', '/api/chat/stream'), payload);
      return;
    }

    this.http.post<ChatResponse>(url, payload).subscribe({
      next: (response) => {
        this.messages = [...this.messages, { role: 'assistant', content: response.message }];
        this.isSending = false;
        this.shouldScroll = true;
      },
      error: (error) => {
        this.isSending = false;
        this.errorMessage =
          error?.error?.error ??
          error?.message ??
          'Unable to reach the backend. Check the API base URL and that the server is running.';
      }
    });
  }

  usePrompt(prompt: string): void {
    this.message = prompt;
  }

  onComposerKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  onCvFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.cvFile = file;
    this.cvFileName = file ? file.name : '';
  }

  clearCv(): void {
    this.cvText = '';
    this.cvFile = null;
    this.cvFileName = '';
    this.cvResult = null;
    this.cvError = '';
  }

  checkCv(): void {
    if (this.cvIsChecking) {
      return;
    }

    const hasText = this.cvText.trim().length > 0;
    const hasFile = !!this.cvFile;
    if (!hasText && !hasFile) {
      this.cvError = 'Please paste resume text or upload a PDF/DOCX file.';
      return;
    }

    this.cvError = '';
    this.cvIsChecking = true;
    this.cvResult = null;

    const formData = new FormData();
    if (hasText) {
      formData.append('text', this.cvText.trim());
    }
    if (this.cvFile) {
      formData.append('file', this.cvFile);
    }

    const baseUrl = this.apiBaseUrl.trim();
    const url = baseUrl ? `${baseUrl}/api/cv/check` : '/api/cv/check';

    this.http.post<CvReview>(url, formData).subscribe({
      next: (result) => {
        this.cvResult = {
          summary: result.summary,
          score: Math.max(0, Math.min(100, result.score)),
          suggestions: result.suggestions ?? []
        };
        this.cvIsChecking = false;
      },
      error: (error) => {
        this.cvIsChecking = false;
        this.cvError =
          error?.error?.error ??
          error?.message ??
          'Unable to reach the backend. Check the API base URL and that the server is running.';
      }
    });
  }

  private async sendStreamingMessage(url: string, payload: ChatRequest): Promise<void> {
    this.streamingAssistantIndex = this.messages.length;
    this.isStreaming = true;
    this.messages = [...this.messages, { role: 'assistant', content: '' }];

    try {
      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Accept: 'text/event-stream'
        },
        body: JSON.stringify(payload)
      });

      if (!response.ok || !response.body) {
        throw new Error(`Streaming failed with status ${response.status}.`);
      }

      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = '';

      while (true) {
        const { value, done } = await reader.read();
        if (done) {
          break;
        }

        buffer += decoder.decode(value, { stream: true });
        const events = buffer.split('\n\n');
        buffer = events.pop() ?? '';

        for (const event of events) {
          const lines = event.split('\n');
          for (const line of lines) {
            if (!line.startsWith('data:')) {
              continue;
            }

            const payloadText = line.replace(/^data:\s*/, '').trim();
            if (!payloadText) {
              continue;
            }

            const parsed = JSON.parse(payloadText) as StreamPayload;
            if (parsed.done) {
              continue;
            }

            if (parsed.delta) {
              this.appendAssistantDelta(parsed.delta);
            }
          }
        }
      }
    } catch (error) {
      this.errorMessage =
        error instanceof Error
          ? error.message
          : 'Unable to reach the backend. Check the API base URL and that the server is running.';
    } finally {
      this.isSending = false;
      this.shouldScroll = true;
      this.streamingAssistantIndex = -1;
      this.isStreaming = false;
    }
  }

  private appendAssistantDelta(delta: string): void {
    if (this.streamingAssistantIndex < 0) {
      return;
    }

    const updated = [...this.messages];
    const current = updated[this.streamingAssistantIndex];
    if (!current) {
      return;
    }

    updated[this.streamingAssistantIndex] = {
      ...current,
      content: `${current.content}${delta}`
    };
    this.messages = updated;
    this.shouldScroll = true;
  }

  private isNearBottom(container: HTMLDivElement): boolean {
    const threshold = 120;
    return container.scrollHeight - container.scrollTop - container.clientHeight < threshold;
  }
}

interface ChatRequest {
  message: string;
  temperature?: number;
  maxTokens?: number;
}

interface ChatResponse {
  message: string;
}

interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

interface StreamPayload {
  delta?: string;
  done?: boolean;
}

interface CvReview {
  summary: string;
  score: number;
  suggestions: string[];
}
