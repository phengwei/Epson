<template>
  <div class="dashboard-container">
    <h1 class="text-3xl font-bold mb-8">Dashboard</h1>
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <nuxt-link v-for="link in dashboardLinks" :key="link.route" :to="link.route" class="dashboard-card">
        <div class="card-content">
          <div class="icon">{{ link.icon }}</div>
          <div class="label">{{ link.label }}</div>
        </div>
      </nuxt-link>
    </div>
  </div>
</template>
<script>
  import { mapGetters } from 'vuex';

  export default {
    name: 'epson-dashboard',
    computed: {
      ...mapGetters(['loggedInUser']),
      dashboardLinks() {
        const links = [];
        if (this.loggedInUser.roles.includes('Admin')) {
          links.push(
            { route: '/userManagement', label: 'Manage Users', icon: '👥' },
            { route: '/categoryManagement', label: 'Manage Product Categories', icon: '📚' },
            { route: '/sla', label: 'SLA Configuration', icon: '⚙️' },
            { route: '/requestRejectionAuditTrail', label: 'Rejection Audit Trail', icon: '📋' },
            { route: '/productAuditTrail', label: 'Product Audit Trail', icon: '📝' },
            { route: '/productDashboard', label: 'Fulfiller Dashboard', icon: '📦' },
            { route: '/salesDashboard', label: 'Requester Dashboard', icon: '📊' },
            { route: '/shDashboard', label: 'Sales Head Dashboard', icon: '🏷️' }
          );
        } else {
          if (this.loggedInUser.roles.includes('Sales')) {
            links.push({ route: '/salesDashboard', label: 'Sales Home', icon: '📊' });
          }
          if (this.loggedInUser.roles.includes('Product') || this.loggedInUser.roles.includes('Coverplus')) {
            links.push({ route: '/productDashboard', label: 'Product Home', icon: '📦' });
          }
          if (this.loggedInUser.roles.includes('Sales Section Head')) {
            links.push({ route: '/shDashboard', label: 'Sales Head Home', icon: '🏷️' });
          }
        }
        return links;
      }
    }
  };
</script>
<style scoped>
  .dashboard-container {
    padding: 20px;
  }

  h1 {
    text-align: center;
    color: #003399;
  }

  .grid {
    display: grid;
  }

  .dashboard-card {
    background: white;
    border-radius: 10px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    padding: 20px;
    transition: transform 0.2s ease-in-out, background-color 0.2s ease-in-out;
    text-align: center;
    text-decoration: none;
    color: inherit;
  }

    .dashboard-card:hover {
      transform: translateY(-10px);
      background-color: #003399;
      color: white;
    }

  .card-content {
    display: flex;
    flex-direction: column;
    align-items: center;
  }

  .icon {
    font-size: 40px;
    margin-bottom: 10px;
  }

  .label {
    font-size: 18px;
    font-weight: bold;
  }
</style>
