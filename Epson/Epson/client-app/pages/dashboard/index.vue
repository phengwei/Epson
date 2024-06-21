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
            { route: '/userManagement', label: 'MANAGE USERS', icon: '👥' },
            { route: '/categoryManagement', label: 'MANAGE PRODUCT CATEGORIES', icon: '📚' },
            { route: '/sla', label: 'SLA CONFIGURATION', icon: '⚙️' },
            { route: '/requestRejectionAuditTrail', label: 'REJECTION AUDIT TRAIL', icon: '📋' },
            { route: '/productAuditTrail', label: 'PRODUCT AUDIT TRAIL', icon: '📝' },
            { route: '/productDashboard', label: 'FULFILLER DASHBOARD', icon: '📦' },
            { route: '/salesDashboard', label: 'REQUESTER DASHBOARD', icon: '📊' },
            { route: '/shDashboard', label: 'SALES HEAD DASHBOARD', icon: '🏷️' }
          );
        } else {
          if (this.loggedInUser.roles.includes('Director')) {
            links.push(
              { route: '/sla', label: 'SLA CONFIGURATION', icon: '⚙️' },
              { route: '/requestRejectionAuditTrail', label: 'REJECTION AUDIT TRAIL', icon: '📋' },
              { route: '/productAuditTrail', label: 'PRODUCT AUDIT TRAIL', icon: '📝' },
              { route: '/productDashboard', label: 'FULFILLER DASHBOARD', icon: '📦' },
              { route: '/salesDashboard', label: 'REQUESTER DASHBOARD', icon: '📊' },
              { route: '/shDashboard', label: 'SALES HEAD DASHBOARD', icon: '🏷️' }
            );
          }
          if (this.loggedInUser.roles.includes('Sales')) {
            links.push({ route: '/salesDashboard', label: 'REQUESTER DASHBOARD', icon: '📊' });
          }
          if (this.loggedInUser.roles.includes('Product') || this.loggedInUser.roles.includes('Coverplus')) {
            links.push({ route: '/productDashboard', label: 'FULFILLER DASHBOARD', icon: '📦' });
          }
          if (this.loggedInUser.roles.includes('Sales Section Head')) {
            links.push({ route: '/shDashboard', label: 'SALES HEAD DASHBOARD', icon: '🏷️' });
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
