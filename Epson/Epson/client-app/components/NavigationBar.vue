<template>
  <nav class="ums-header bg-[#19212b] top-0 inset-x-0 w-full z-30 text-white fixed transition duration-300 delay-0 ease-out">
    <div class="flex sm:px-18 px-4">
      <div class="w-full flex justify-between">
        <div class="flex md:space-x-7 md:w-full justify-start">
          <!-- Dashboard Dropdown and Primary Navbar items -->
          <div class="hidden md:flex items-center w-full" v-if="isAuthenticated">
            <nuxt-link to="/user"
                       class="w-40 h-full hover:bg-[#003399] flex justify-center items-center font-semibold transition duration-300">{{ loggedInUser.userName }}</nuxt-link>

            <!-- Dashboard Dropdown -->
            <div class="relative group" @click="toggleDashboardDropdown" ref="dashboardDropdown" v-if="!loggedInUser.roles.includes('Sales Operation')">
              <span class="w-40 h-full flex justify-center items-center font-semibold transition duration-300 cursor-pointer">Dashboard</span>
              <div class="absolute left-0 mt-1 w-48 rounded-md shadow-lg py-1 bg-white text-black z-50" :class="{ 'hidden': !showDashboardDropdown }">
                <nuxt-link v-for="link in dashboardLinks"
                           :to="link.route"
                           :key="link.route"
                           class="block px-4 py-2 hover:bg-[#003399] hover:text-white">
                  {{ link.label }}
                </nuxt-link>
              </div>
            </div>
            <nuxt-link v-if="loggedInUser.roles.includes('Admin') || loggedInUser.roles.includes('Product')" to="/reporting"
                       class="w-40 h-full hover:bg-[#003399] flex justify-center items-center font-semibold transition duration-300">Report</nuxt-link>
            <nuxt-link v-if="loggedInUser.roles.includes('Admin') || loggedInUser.roles.includes('Product') || loggedInUser.roles.includes('Coverplus') || loggedInUser.roles.includes('Sales Section Head')" to="/slaDashboard"
                       class="w-40 h-full hover:bg-[#003399] flex justify-center items-center font-semibold transition duration-300">SLA Overview</nuxt-link>
            <nuxt-link v-if="loggedInUser.roles.includes('Sales') || loggedInUser.roles.includes('Sales Section Head') || loggedInUser.roles.includes('Sales Operation')" to="/request"
                       class="w-40 h-full hover:bg-[#003399] flex justify-center items-center font-semibold transition duration-300">Requests</nuxt-link>
            <nuxt-link v-if="loggedInUser.roles.includes('Product') || loggedInUser.roles.includes('Admin')" to="/product"
                       class="w-40 h-full hover:bg-[#003399] flex justify-center items-center font-semibold transition duration-300">Products</nuxt-link>

            <!-- Admin Center Dropdown -->
            <div class="relative group" @click="toggleDropdown" ref="dropdown" v-if="loggedInUser.roles.includes('Admin')">
              <span class="w-40 h-full flex justify-center items-center font-semibold transition duration-300 cursor-pointer">Admin Center</span>
              <div class="absolute left-0 mt-1 w-48 rounded-md shadow-lg py-1 bg-white text-black z-50" :class="{ 'hidden': !showDropdown }">
                <nuxt-link to="/userManagement"
                           class="block px-4 py-2 hover:bg-[#003399] hover:text-white">Manage Users</nuxt-link>
                <nuxt-link to="/categoryManagement"
                           class="block px-4 py-2 hover:bg-[#003399] hover:text-white">Manage Product Categories</nuxt-link>
                <nuxt-link to="/sla"
                           class="block px-4 py-2 hover:bg-[#003399] hover:text-white">SLA Configuration</nuxt-link>
                <nuxt-link to="/productAuditTrail"
                           class="block px-4 py-2 hover:bg-[#003399] hover:text-white">Product Audit Trail</nuxt-link>
                <nuxt-link to="/productDashboard"
                           class="block px-4 py-2 hover:bg-[#003399] hover:text-white">Product Dashboard</nuxt-link>
                <nuxt-link to="/salesDashboard"
                           class="block px-4 py-2 hover:bg-[#003399] hover:text-white">Sales Dashboard</nuxt-link>
                <nuxt-link to="/shDashboard"
                           class="block px-4 py-2 hover:bg-[#003399] hover:text-white">Sales Section Dashboard</nuxt-link>
              </div>
            </div>

            <a class="w-40 h-full hover:bg-[#003399] flex justify-center items-center font-semibold transition duration-300 cursor-pointer"
               @click="logout">Logout</a>
          </div>
        </div>

        <!-- Website Logo -->
        <div class="flex items-center">
          <nuxt-link to="/" class="py-4 px-2 w-[150px]">
            <img class="w-[150px] h-14 object-contain"
                 src="/images/svg/epson-logo.png"
                 alt="Epson" />
          </nuxt-link>
        </div>

        <!-- Mobile menu button -->
        <div class="md:hidden flex items-center">
          <button class="outline-none" @click="openPopup()">
            <svg class="w-6 h-6 text-white"
                 x-show="!showMenu"
                 fill="none"
                 stroke-linecap="round"
                 stroke-linejoin="round"
                 stroke-width="2"
                 viewBox="0 0 24 24"
                 stroke="currentColor">
              <path d="M4 6h16M4 12h16M4 18h16"></path>
            </svg>
          </button>
        </div>
      </div>
    </div>
  </nav>
</template>

<script>
  import { mapGetters } from 'vuex';

  export default {
    name: 'HeaderNav',
    data() {
      return {
        showDashboardDropdown: false,
        showDropdown: false
      };
    },
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser']),
      dashboardLinks() {
        const links = [];
        if (this.loggedInUser.roles.includes('Admin')) {
          links.push({ route: '/userManagement', label: 'User Management' });
        }
        if (this.loggedInUser.roles.includes('Sales')) {
          links.push({ route: '/salesDashboard', label: 'Sales Dashboard' });
        }
        if (this.loggedInUser.roles.includes('Product') || this.loggedInUser.roles.includes('Coverplus')) {
          links.push({ route: '/productDashboard', label: 'Product Dashboard' });
        }
        if (this.loggedInUser.roles.includes('Sales Section Head')) {
          links.push({ route: '/shDashboard', label: 'Sales Section Dashboard' });
        }
        return links;
      }
    },
    methods: {
      toggleDashboardDropdown() {
        this.showDashboardDropdown = !this.showDashboardDropdown;
      },
      toggleDropdown() {
        this.showDropdown = !this.showDropdown;
      },
      logout() {
        this.$auth.logout().then(() => {
          localStorage.clear();
          this.$router.push('/login');
          this.$router.go(0);
        });
      }
    }
  };
</script>

<style>
  .homeHeader {
    background-color: #f1f3f5;
    color: rgb(107 114 128);
  }

  .homePageMobile {
    background-color: #f1f3f5;
  }

  .homePageIsActive {
    color: #003399;
    border-color: #003399;
    border-bottom-width: 4px;
  }

  .homePageIsActiveScrolled {
    color: #003399;
    border-color: #003399;
  }

  .homePageIsActiveMobile {
    background-color: #003399;
    color: white
  }

  .stop-scrolling {
    height: 100%;
    overflow: hidden
  }

  .transition-right {
    transform: translate(100%, 0);
  }
</style>
