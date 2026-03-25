import type {SidebarsConfig} from '@docusaurus/plugin-content-docs';

const sidebars: SidebarsConfig = {
  docsSidebar: [
    {
      type: 'doc',
      id: 'index',
      label: 'Welcome',
    },
    {
      type: 'category',
      label: 'Getting Started',
      link: {type: 'doc', id: 'getting-started/index'},
      items: [
        'getting-started/logging-in',
        'getting-started/your-profile',
        'getting-started/navigation',
        'getting-started/prerequisites',
        'getting-started/installation',
        'getting-started/configuration',
        'getting-started/local-development',
      ],
    },
    {
      type: 'category',
      label: 'User Guide',
      link: {type: 'doc', id: 'user-guide/index'},
      items: [
        {
          type: 'doc',
          id: 'user-guide/organizations/index',
          label: 'Organizations',
        },
        {
          type: 'category',
          label: 'Planning',
          link: {type: 'doc', id: 'user-guide/planning/index'},
          items: [
            'user-guide/planning/planning-intervals',
            'user-guide/planning/sprints',
            'user-guide/planning/risks',
            'user-guide/planning/roadmaps',
            'user-guide/planning/planning-poker',
          ],
        },
        {
          type: 'category',
          label: 'Work Management',
          link: {type: 'doc', id: 'user-guide/work-management/index'},
          items: [
            'user-guide/work-management/work-items',
            'user-guide/work-management/work-configuration',
          ],
        },
        {
          type: 'category',
          label: 'Project Portfolios',
          link: {type: 'doc', id: 'user-guide/project-portfolios/index'},
          items: [
            'user-guide/project-portfolios/portfolios-programs',
            'user-guide/project-portfolios/projects',
            'user-guide/project-portfolios/strategic-initiatives',
            'user-guide/project-portfolios/my-projects',
          ],
        },
        {
          type: 'doc',
          id: 'user-guide/strategic-management/index',
          label: 'Strategic Management',
        },
        {
          type: 'doc',
          id: 'user-guide/settings/index',
          label: 'Administration',
        },
      ],
    },
  ],

  contributingSidebar: [
    {
      type: 'category',
      label: 'Contributing',
      link: {type: 'doc', id: 'contributing/index'},
      items: [
        'contributing/architecture',
        'contributing/coding-standards',
        'contributing/testing',
        'contributing/adding-a-feature',
        'contributing/database',
        'contributing/frontend',
        'contributing/api',
      ],
    },
  ],

  referenceSidebar: [
    {
      type: 'category',
      label: 'Reference',
      link: {type: 'doc', id: 'reference/index'},
      items: [
        'reference/domain-model',
        'reference/api',
        'reference/feature-flags',
        'reference/integrations',
        'reference/technology-stack',
      ],
    },
    {
      type: 'category',
      label: 'AI Context',
      items: [
        'ai/domain-glossary',
      ],
    },
  ],
};

export default sidebars;
