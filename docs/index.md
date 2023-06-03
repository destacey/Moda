# Welcome to Moda
Moda is a work management system used to plan, manage, and create associations across work items, projects, teams, planning and products.  It helps track, align, and deliver work across organizations.

## Domains
- [Work](./work.md)
- [Organization](./organization.md)
- [Planning](./planning.md)
- Product Delivery
- Project Portfolio Management
- [App Integration](./app-integration.md)

## Ownership
Moda is built to allow data from external systems to be synchronized into Moda.  This allows organizations to collect and view all of their data related to teams and work management in one location.  Objects in Moda that can come from external systems will have an Ownership attribute.  The options for that attribute are:
- Owned - The object is owned by Moda.
- Managed - The object is owned by an external system.

Objects that are managed by Moda are read only and cannot be changed from within Moda.  Those objects can be associated or linked to other owned items within Moda.  An example of this would be a work item owned by Azure DevOps, and thus managed by Moda, can be linked to a Team Objective owned by Moda.