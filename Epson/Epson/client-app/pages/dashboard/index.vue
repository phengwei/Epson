<template>
  <v-app>
    <v-main>
      <div class="dashboard-container">
        <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;">
          <v-card-title>
            <v-toolbar flat>
              <v-toolbar-title>
                <h2 class="blue-text big-bold">OVERVIEW</h2>
              </v-toolbar-title>
            </v-toolbar>
          </v-card-title>
          <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-3 gap-6">
            <v-menu v-if="categoryLinks.length > 0" offset-y>
              <template v-slot:activator="{ on, attrs }">
                <v-btn class="dashboard-card" v-bind="attrs" v-on="on">
                  <div class="card-content">
                    <div class="label">CATEGORIES</div>
                  </div>
                </v-btn>
              </template>
              <v-list>
                <nuxt-link v-for="link in categoryLinks" :key="link.route" :to="link.route">
                  <v-list-item>
                    <v-list-item-title>{{ link.label }}</v-list-item-title>
                  </v-list-item>
                </nuxt-link>
              </v-list>
            </v-menu>

            <v-menu v-if="auditTrailLinks.length > 0" offset-y>
              <template v-slot:activator="{ on, attrs }">
                <v-btn class="dashboard-card" v-bind="attrs" v-on="on">
                  <div class="card-content">
                    <div class="label">AUDIT TRAIL</div>
                  </div>
                </v-btn>
              </template>
              <v-list>
                <nuxt-link v-for="link in auditTrailLinks" :key="link.route" :to="link.route">
                  <v-list-item>
                    <v-list-item-title>{{ link.label }}</v-list-item-title>
                  </v-list-item>
                </nuxt-link>
              </v-list>
            </v-menu>

            <v-menu v-if="dashboardLinks.length > 0" offset-y>
              <template v-slot:activator="{ on, attrs }">
                <v-btn class="dashboard-card" v-bind="attrs" v-on="on">
                  <div class="card-content">
                    <div class="label">DASHBOARDS</div>
                  </div>
                </v-btn>
              </template>
              <v-list>
                <nuxt-link v-for="link in dashboardLinks" :key="link.route" :to="link.route">
                  <v-list-item>
                    <v-list-item-title>{{ link.label }}</v-list-item-title>
                  </v-list-item>
                </nuxt-link>
              </v-list>
            </v-menu>
          </div>
        </v-card>
      </div>
    </v-main>
  </v-app>
</template>

<script>
  import { mapGetters } from 'vuex';

  export default {
    name: 'epson-dashboard',
    computed: {
      ...mapGetters(['loggedInUser']),
      categoryLinks() {
        const links = [];
        if (this.loggedInUser.roles.includes('Admin')) {
          links.push(
            { route: '/userManagement', label: 'MANAGE USERS', icon: '👥' },
            { route: '/categoryManagement', label: 'MANAGE PRODUCT CATEGORIES', icon: '📚' },
            { route: '/sla', label: 'SLA CONFIGURATION', icon: '⚙️' },
            { route: '/hierarchy', label: 'ROUTE CONFIGURATION', icon: '⚙️' }
          );
        }
        if (this.loggedInUser.roles.includes('Sales') || this.loggedInUser.roles.includes('Admin') || this.loggedInUser.roles.includes('Sales Section Head') || this.loggedInUser.roles.includes('Director')) {
          links.push(
            { route: '/draft', label: 'DRAFT MANAGEMENT', icon: 'mdi-file-document-edit-outline' },
          );
        }
        return links;
      },
      auditTrailLinks() {
        const links = [];
        if (this.loggedInUser.roles.includes('Admin') || this.loggedInUser.roles.includes('Director')) {
          links.push(
            { route: '/requestAuditTrail', label: 'REQUEST AUDIT TRAIL', icon: '📋' },
            { route: '/productAuditTrail', label: 'PRODUCT AUDIT TRAIL', icon: '📝' }
          );
        }
        return links;
      },
      dashboardLinks() {
        const links = [];
        if (this.loggedInUser.roles.includes('Admin') || this.loggedInUser.roles.includes('Director')) {
          links.push(
            { route: '/productDashboard', label: 'FULFILLER DASHBOARD', icon: '📦' },
            { route: '/salesDashboard', label: 'REQUESTER DASHBOARD', icon: '📊' },
            { route: '/shDashboard', label: 'SALES HEAD DASHBOARD', icon: '🏷️' }
          );
        } else {
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
  .v-application {
    font-family: TCCC-UnityText-Regular, TCCC-UnityText !important;
  }

  .dashboard-container {
    padding: 20px;
  }

  h2 {
    text-align: center;
    color: #003399;
  }

  .grid {
    display: grid;
  }

  .dashboard-card {
    background: #003399 !important;
    color: white;
    border-radius: 10px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    padding: 20px;
    transition: transform 0.2s ease-in-out, background-color 0.2s ease-in-out;
    text-align: center;
    text-decoration: none;
    cursor: pointer;
  }

    .dashboard-card:hover {
      transform: translateY(-10px);
      background-color: #002080;
    }

  .card-content {
    display: flex;
    flex-direction: column;
    align-items: center;
  }

  .label {
    font-size: 18px;
    font-weight: bold;
    color: white;
  }

  .blue-text {
    color: #003399;
    font-weight: bold;
  }

  .theme--dark.v-btn.v-btn--has-bg {
    background-color: #003399 !important;
  }

  .big-bold {
    font-size: 1.5rem;
  }

  .v-menu__content {
    border-radius: 10px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  }
</style>
