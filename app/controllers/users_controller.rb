class UsersController < ApplicationController
	 
def index
    @users = User.all
  end
	 def show
    @user = User.find(params[:id])
  end

	def new
  end

  def create
  @user = User.new(user_params)
 #Rails.logger.debug @user.inspect
  @user.save
  redirect_to @user
end
 
private
  def user_params
    params.require(:user).permit(:nick, :login, :password_digest, :email)
  end
end
