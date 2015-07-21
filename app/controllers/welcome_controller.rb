class WelcomeController < ApplicationController
  def index
  @articles = Article.all
   if signed_in?
      #@rticle  = current_user.articles.build
      #@feed_items = current_user.feed.paginate(page: params[:page])
   
  end
  end

  def contact
  end
end
