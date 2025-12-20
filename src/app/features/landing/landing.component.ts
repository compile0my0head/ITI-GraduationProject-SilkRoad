import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

interface Feature {
  icon: string;
  title: string;
  description: string;
}

interface Step {
  number: string;
  title: string;
  description: string;
}

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.scss']
})
export class LandingComponent {
  activeFeature = signal(0);
  currentYear = new Date().getFullYear();

  features: Feature[] = [
    {
      icon: 'M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z',
      title: 'AI Content Generation',
      description: 'Generate compelling social media posts, product descriptions, and marketing copy powered by advanced AI that understands your brand voice.'
    },
    {
      icon: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',
      title: 'Smart Campaign Scheduling',
      description: 'Schedule and automate your marketing campaigns across multiple platforms. Reach your audience at the optimal time for maximum engagement.'
    },
    {
      icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z',
      title: 'Sales Analytics Dashboard',
      description: 'Track revenue, orders, and customer behavior with real-time analytics. Make data-driven decisions to grow your business faster.'
    },
    {
      icon: 'M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z',
      title: 'AI Chatbot Automation',
      description: 'Engage customers 24/7 with an intelligent chatbot that handles inquiries, processes orders, and provides instant support on Facebook Messenger.'
    },
    {
      icon: 'M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15',
      title: 'Workflow Automation',
      description: 'Automate repetitive tasks like order confirmations, inventory updates, and customer follow-ups. Save hours every week.'
    }
  ];

  steps: Step[] = [
    {
      number: '01',
      title: 'Connect Your Store',
      description: 'Link your online store and social media accounts in minutes. We support Facebook integration with more platforms coming soon.'
    },
    {
      number: '02',
      title: 'Create Campaigns',
      description: 'Use AI to generate content or create your own. Schedule posts, set up automation rules, and configure your chatbot responses.'
    },
    {
      number: '03',
      title: 'Grow on Autopilot',
      description: 'Let SilkRoad handle customer engagement, track your sales, and optimize your campaigns while you focus on what matters most.'
    }
  ];

  setActiveFeature(index: number): void {
    this.activeFeature.set(index);
  }

  scrollToSection(sectionId: string): void {
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }
}
