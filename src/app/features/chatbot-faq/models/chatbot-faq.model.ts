export interface ChatbotFAQ {
  faqID: number;
  storeID: number;
  question: string;
  answer: string;
  createdAt: Date;
}

export interface CreateChatbotFAQRequest {
  question: string;
  answer: string;
}

export interface UpdateChatbotFAQRequest {
  question?: string;
  answer?: string;
}
