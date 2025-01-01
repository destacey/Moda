# Contributing to Moda

Thank you for your interest in contributing to Moda! We welcome contributions from the community to improve the project and make it better for everyone.

## How to Contribute

### 1. Reporting Issues
If you encounter a bug, have a question, or want to suggest an enhancement, please [create an issue](https://github.com/destacey/Moda/issues). When creating an issue:
- Provide a clear and descriptive title.
- Explain the issue in detail, including steps to reproduce if applicable.
- Attach relevant screenshots or code snippets if necessary.

### 2. Suggesting Features
We’re always looking for ways to improve Moda. If you have an idea for a new feature:
- Check if it’s already been suggested by browsing existing issues.
- If it hasn’t been suggested, [open a new issue](https://github.com/destacey/Moda/issues) and describe your idea clearly.

### 3. Submitting Code Changes
#### Fork and Clone the Repository
Non-maintainers will need to fork the repository before making changes. Pull requests are required for all contributions.
1. Fork the repository to your own GitHub account.
2. Clone your fork locally:
   ```bash
   git clone https://github.com/your-username/Moda.git
   cd Moda
   ```

#### Set Up Your Environment
Follow the setup instructions in the `README.md` file to get the project running locally.

#### Create a Branch
Create a new branch for your changes:
```bash
git checkout -b feature/your-feature-name
```
Use a descriptive branch name that reflects the purpose of your changes.

#### Make Your Changes
Make your changes to the codebase. Ensure that:
- Your code adheres to the project’s coding standards and style.
- You write tests for any new functionality.

#### Run Tests
Run the test suite to ensure your changes don’t break anything:
```bash
npm test
```

#### Commit Your Changes
Commit your changes with a meaningful commit message:
```bash
git add .
git commit -m "Add feature: description of feature"
```
If your changes address an existing issue, make sure to link to the issue in your commit message or pull request description, e.g., `Fixes #<issue-number>`.

#### Push Your Changes
Push your branch to your fork:
```bash
git push origin feature/your-feature-name
```

#### Create a Pull Request
1. Go to the [original repository](https://github.com/destacey/Moda) on GitHub.
2. Click the **New Pull Request** button.
3. Select your branch and provide a detailed description of your changes, including links to any related issues.

### 4. Reviewing and Feedback
Your pull request will be reviewed by the maintainers. Please be prepared to:
- Make changes if requested.
- Discuss your implementation or suggest alternatives.

## Code Style and Guidelines
To maintain consistency across the project:
- Follow the existing coding conventions in the project.
- Use meaningful variable and function names.
- Write clear, concise comments where necessary.

## Getting Help
If you need help while contributing, feel free to reach out by:
- Commenting on an existing issue related to your question.
- Opening a new issue with your question or concern.

Thank you for contributing to Moda! Together, we can build something amazing.
